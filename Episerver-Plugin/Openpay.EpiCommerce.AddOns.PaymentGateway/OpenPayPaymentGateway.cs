using Openpay.EpiCommerce.AddOns.PaymentGateway.Models;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Globalization;
using System.Linq;
using System.Net;
using EPiServer.Commerce.Order;
using EPiServer.Security;
using Mediachase.Commerce.Orders;
using Mediachase.Commerce.Plugins.Payment;
using Mediachase.Commerce.Security;
using Openpay.EpiCommerce.AddOns.PaymentGateway.Constants;
using Openpay.EpiCommerce.AddOns.PaymentGateway.Enums;
using Openpay.EpiCommerce.AddOns.PaymentGateway.Helpers;
using System.Web;
using EPiServer;
using EPiServer.Core;
using EPiServer.Data;
using EPiServer.Logging;
using EPiServer.ServiceLocation;
using EPiServer.Web;
using EPiServer.Web.Routing;
using Mediachase.Commerce;
using Mediachase.Commerce.Core;
using Mediachase.Commerce.Core.Features;
using Mediachase.Commerce.Customers;
using Mediachase.Commerce.Extensions;
using Mediachase.Commerce.Orders.Exceptions;
using Mediachase.Commerce.Orders.Managers;
using Openpay.EpiCommerce.AddOns.PaymentGateway.Models.CreationNewOrderRequest;

namespace Openpay.EpiCommerce.AddOns.PaymentGateway
{
    public class OpenpayPaymentGateway : AbstractPaymentGateway, IPaymentPlugin
    {
        private readonly OpenpayConfiguration _openpayConfiguration;
        private readonly IPaymentProcessor _paymentProcessor;
        private readonly IOrderGroupCalculator _orderGroupCalculator;
        private readonly IOrderRepository _orderRepository;
        private static Lazy<DatabaseMode> _databaseMode = new Lazy<DatabaseMode>(GetDefaultDatabaseMode);
        private readonly IInventoryProcessor _inventoryProcessor;
        private readonly IFeatureSwitch _featureSwitch;
        private readonly IOrderNumberGenerator _orderNumberGenerator;
        private readonly IOrderGroupFactory _orderGroupFactory;

        private ILogger _logger
        {
            get
            {
                Func<ILoggerFactory> loggerFactory = LogManager.LoggerFactory;
                var factory = loggerFactory();
                return factory.Create(CommonConstants.OpenpayLogAppender);
            }
        }

        public OpenpayPaymentGateway() : this(
            ServiceLocator.Current.GetInstance<IFeatureSwitch>(),
            ServiceLocator.Current.GetInstance<IInventoryProcessor>(),
            ServiceLocator.Current.GetInstance<IOrderNumberGenerator>(),
            ServiceLocator.Current.GetInstance<IOrderGroupCalculator>(),
            ServiceLocator.Current.GetInstance<IPaymentProcessor>(),
            ServiceLocator.Current.GetInstance<IOrderGroupFactory>(),
            ServiceLocator.Current.GetInstance<IOrderRepository>()
        )
        {
        }

        public OpenpayPaymentGateway(
            IFeatureSwitch featureSwitch,
            IInventoryProcessor inventoryProcessor,
            IOrderNumberGenerator orderNumberGenerator,
            IOrderGroupCalculator orderGroupCalculator,
            IPaymentProcessor paymentProcessor,
            IOrderGroupFactory orderGroupFactory,
            IOrderRepository orderRepository
        )
        {
            _featureSwitch = featureSwitch;
            _inventoryProcessor = inventoryProcessor;
            _orderNumberGenerator = orderNumberGenerator;
            _openpayConfiguration = new OpenpayConfiguration(Settings);
            _orderGroupCalculator = orderGroupCalculator;
            _orderRepository = orderRepository;
            _paymentProcessor = paymentProcessor;
            _orderGroupFactory = orderGroupFactory;
        }

        public override bool ProcessPayment(Payment payment, ref string message)
        {
            var paymentProcessingResult = ProcessPayment(payment.Parent.Parent, payment);
            message = paymentProcessingResult.Message;
            return paymentProcessingResult.IsSuccessful;
        }

        /// <summary>
        /// Processes the payment. Can be used for both positive and negative transactions.
        /// </summary>
        /// <param name="orderGroup">The order group.</param>
        /// <param name="payment">The payment.</param>
        /// <returns>The payment processing result.</returns>
        public PaymentProcessingResult ProcessPayment(IOrderGroup orderGroup, IPayment payment)
        {
            if (HttpContext.Current == null)
            {
                return PaymentProcessingResult.CreateSuccessfulResult(Utilities.Translate("ProcessPaymentNullHttpContext"));
            }

            if (payment == null)
            {
                return PaymentProcessingResult.CreateUnsuccessfulResult(Utilities.Translate("PaymentNotSpecified"));
            }

            var orderForm = orderGroup.Forms.FirstOrDefault(f => f.Payments.Contains(payment));
            if (orderForm == null)
            {
                return PaymentProcessingResult.CreateUnsuccessfulResult(Utilities.Translate("PaymentNotAssociatedOrderForm"));
            }

            PaymentProcessingResult paymentProcessingResult;
            var cart = orderGroup as ICart;
            // the order which is created by Commerce Manager
            if (cart == null && orderGroup is IPurchaseOrder)
            {
                // When "Refund" shipment in Commerce Manager, this method will be invoked with the TransactionType is Credit
                if (payment.TransactionType == TransactionType.Credit.ToString())
                {
                    paymentProcessingResult = ProcessPaymentRefund(orderGroup, payment);

                    return paymentProcessingResult;
                }

                // Orders with Openpay Payment have to be paid before processing if not don't process this order.
                if (payment.TransactionType == TransactionType.Capture.ToString())
                {
                    var capturePayments = orderGroup.Forms.SelectMany(x => x.Payments)
                        .Count(y => y.TransactionType == TransactionType.Capture.ToString() && y.Status == PaymentStatus.Processed.ToString());

                    if(capturePayments > 0)
                    {
                        paymentProcessingResult = PaymentProcessingResult.CreateSuccessfulResult("This order has been paid in Openpay already!");
                    }
                    else
                    {
                        paymentProcessingResult = PaymentProcessingResult.CreateUnsuccessfulResult("Cannot finish this order because it was failed to be paid in Openpay at the beginning.");
                    }

                    return paymentProcessingResult;
                }

                // right now we do not support processing the order which is created by Commerce Manager
                paymentProcessingResult = PaymentProcessingResult.CreateUnsuccessfulResult("The current payment method does not support order type.");
                return paymentProcessingResult; // raise exception
            }

            // Validate cart
            var priceLimitValid = Utilities.ValidatePurchasePriceLimit(payment.Amount, _openpayConfiguration);
            if (!priceLimitValid)
            {
                return PaymentProcessingResult.CreateUnsuccessfulResult(Utilities.Translate("InvalidPurchasePrice"));
            }

            var excludeProductValid = Utilities.ValidateProhibitedProducts(cart, _openpayConfiguration.ExcludedProductConfigItem);
            if (!excludeProductValid)
            {
                return PaymentProcessingResult.CreateUnsuccessfulResult(Utilities.Translate("IncludeProhibitedProduct"));
            }

            // CHECKOUT
            paymentProcessingResult = ProcessPaymentCheckout(cart, payment, orderForm);

            return paymentProcessingResult;
        }


        #region Checkout payment process

        /// <summary>
        /// Process payment when user checkout.
        /// </summary>
        /// <param name="cart">The current cart.</param>
        /// <param name="payment">The payment to process.</param>
        /// <returns>return false and set the message will make the WorkFlow activity raise PaymentExcetion(message)</returns>
        private PaymentProcessingResult ProcessPaymentCheckout(ICart cart, IPayment payment, IOrderForm orderForm)
        {
            if (string.IsNullOrWhiteSpace(_openpayConfiguration.Username)
                || string.IsNullOrWhiteSpace(_openpayConfiguration.Password)
                || string.IsNullOrWhiteSpace(_openpayConfiguration.MinPurchaseLimit)
                || string.IsNullOrWhiteSpace(_openpayConfiguration.MaxPurchaseLimit)
                || string.IsNullOrWhiteSpace(_openpayConfiguration.CallbackItemId))
            {
                return PaymentProcessingResult.CreateUnsuccessfulResult(Utilities.Translate("OpenpaySettingsError"));
            }
            var merchantOrderId = _orderNumberGenerator.GenerateOrderNumber(cart);

            var requestModel = GetRequestModel(cart, orderForm, payment, merchantOrderId);
            if (requestModel == null)
            {
                return PaymentProcessingResult.CreateUnsuccessfulResult(Utilities.Translate("InvalidCustomerInfo"));
            }

            try
            {
                var response = OpenpayApiHelper.SendNewOrderCreationRequest(requestModel, _openpayConfiguration);
                // save response detail to payment
                payment.Properties[OpenpayConfigurationConstants.OpenpayOrderId] = response.OrderId;
                payment.Properties[OpenpayConfigurationConstants.MerchantOrderId] = merchantOrderId;

                if (response.NextAction.Type == NextActionType.FormPost && response.NextAction.FormPost.FormFields != null)
                {
                    var transactionToken = response.NextAction.FormPost.FormFields.FirstOrDefault(x =>
                        x.FieldName == CommonConstants.ResponseFormFields.TransactionToken);
                    if (!string.IsNullOrWhiteSpace(transactionToken?.FieldValue))
                    {
                        _orderRepository.Save(cart);
                        var handoverUrl = UriUtil.AddQueryString(response.NextAction.FormPost.FormPostUrl,
                            CommonConstants.ResponseFormFields.TransactionToken, transactionToken.FieldValue);
                        return PaymentProcessingResult.CreateSuccessfulResult("", handoverUrl);
                    }
                }
            }
            catch (Exception e)
            {
                _logger.Error(e.Message + e.InnerException);
            }

            return PaymentProcessingResult.CreateUnsuccessfulResult(Utilities.Translate("CheckoutProcessError"));
        }

        private CreationNewOrderRequest GetRequestModel(ICart cart, IOrderForm orderForm, IPayment payment, string merchantOrderId)
        {
            // Validation customer info
            var shippingAddress = orderForm.Shipments.FirstOrDefault();
            var billingAddress = payment.BillingAddress;

            var customerValidation = ValidateCustomerInfo(billingAddress, shippingAddress?.ShippingAddress);
            if (!customerValidation)
            {
                return null;
            }

            #region Preparation

            // build public callback url
            var callbackItemRef = new PageReference(_openpayConfiguration.CallbackItemId);
            var routeHelper = ServiceLocator.Current.GetInstance<IPageRouteHelper>();
            var pageUrl = UrlResolver.Current.GetUrl(callbackItemRef,
                routeHelper.LanguageID,
                new VirtualPathArguments { ContextMode = ContextMode.Default });
            
            var absoluteUrl = new Uri($"{HttpContext.Current.Request.Url.Scheme}://{HttpContext.Current.Request.Url.Authority}{pageUrl}");

            // build cart model
            var carts = new List<CartItem>();
            var currency = cart.Currency;
            foreach (var lineItem in orderForm.GetAllLineItems())
            {
                // recalculate final unit price after all kind of discounts are subtracted from item.ListPrice
                var finalUnitPrice = currency.Round(lineItem.GetExtendedPrice(currency).Amount / lineItem.Quantity);
                var lineItemTotal = finalUnitPrice * lineItem.Quantity;

                if (!string.IsNullOrWhiteSpace(lineItem.DisplayName) && !string.IsNullOrWhiteSpace(lineItem.Code))
                {
                    carts.Add(new CartItem
                    {
                        ItemName = lineItem.DisplayName,
                        ItemCode = lineItem.Code,
                        ItemRetailUnitPrice = Utilities.ToOpenpayPrice(finalUnitPrice),
                        ItemQty = Convert.ToInt32(lineItem.Quantity.ToString("0")),
                        ItemRetailCharge = Utilities.ToOpenpayPrice(lineItemTotal),
                    });
                }
            }

            // build delivery address model
            Address deliveryAddress = null;
            if (shippingAddress?.ShippingAddress != null)
            {
                deliveryAddress = new Address
                {
                    Line1 = shippingAddress.ShippingAddress.Line1,
                    Line2 = shippingAddress.ShippingAddress.Line2,
                    Suburb = shippingAddress.ShippingAddress.City,
                    State = shippingAddress.ShippingAddress.RegionCode,
                    PostCode = shippingAddress.ShippingAddress.PostalCode
                };
            }
            

            // build residential address model
            var residentialAddress = new Address
            {
                Line1 = billingAddress.Line1,
                Line2 = billingAddress.Line2,
                Suburb = billingAddress.City,
                State = billingAddress.RegionCode,
                PostCode = billingAddress.PostalCode
            };

            #endregion

            var requestModel = new CreationNewOrderRequest
            {
                CustomerJourney = new CustomerJourney
                {
                    Origin = OrderOrigin.Online.ToString(),
                    Online = new OnlineOrigin
                    {
                        CallbackUrl = absoluteUrl.AbsoluteUri,
                        CancelUrl = absoluteUrl.AbsoluteUri,
                        FailUrl = absoluteUrl.AbsoluteUri,
                        PlanCreationType = CommonConstants.PlanCreationTypeValue,
                        CustomerDetails = new CustomerDetails
                        {
                            FirstName = billingAddress.FirstName,
                            FamilyName = billingAddress.LastName,
                            Email = billingAddress.Email,
                            PhoneNumber = billingAddress.DaytimePhoneNumber,
                            ResidentialAddress = residentialAddress,
                            DeliveryAddress = deliveryAddress
                        },
                        DeliveryMethod = deliveryAddress == null ? DeliveryMethod.Email.ToString() : DeliveryMethod.Delivery.ToString()
                    }
                },
                PurchasePrice = Utilities.ToOpenpayPrice(payment.Amount),
                RetailerOrderNo = merchantOrderId,
                Source = CommonConstants.EpiserverBrandName,
                Cart = carts
            };

            return requestModel;
        }

        #endregion


        #region Refund payment process

        /// <summary>
        /// Processing Refund Payment
        /// </summary>
        /// <param name="orderGroup"></param>
        /// <param name="payment"></param>
        /// <returns></returns>
        private PaymentProcessingResult ProcessPaymentRefund(IOrderGroup orderGroup, IPayment payment)
        {
            var reducePriceBy = Utilities.ToOpenpayPrice(payment.Amount);
            var purchaseOrder = (orderGroup as IPurchaseOrder);
            if (purchaseOrder == null || reducePriceBy <= 0)
            {
                return RefundError(Utilities.Translate("OpenpayRefundError"), purchaseOrder);
            }

            var openpayOrderId = (string)orderGroup.Properties[OpenpayConfigurationConstants.OpenpayOrderId];
            // Check status of Openpay plan
            var orderStatus = OpenpayApiHelper.GetOrderStatusApi(openpayOrderId, _openpayConfiguration);
            if (orderStatus == null || orderStatus.PlanStatus != OpenpayPlanStatus.Active.ToString())
            {
                return RefundError(Utilities.Translate("PlanStatusIsNotActive"), purchaseOrder);
            }

            var newPurchasePrice = orderStatus.PurchasePrice - reducePriceBy;
            var fullRefund = newPurchasePrice == 0;

            // Openpay doesn't support refund more than purchase price
            if (newPurchasePrice >= orderStatus.PurchasePrice || reducePriceBy > orderStatus.PurchasePrice)
            {
                return RefundError(Utilities.Translate("RefundMoreThanPurchasePrice"), purchaseOrder);
            }

            // Call payment gateway API to do refund business
            string responseOrderId;
            try
            {
                responseOrderId = OpenpayApiHelper.SendRefundRequest(openpayOrderId, newPurchasePrice, 
                    reducePriceBy, fullRefund, _openpayConfiguration);
            }
            catch (Exception e)
            {
                _logger.Error($"{e.Message}. {e.InnerException}");
                return RefundError(e.Message, purchaseOrder);
            }

            // Extract the response details.
            payment.TransactionID = responseOrderId;
            var message = $"[{payment.PaymentMethodName}] [RefundTransaction-{responseOrderId}] ";

            // add a new order note about this refund
            Utilities.AddNoteToPurchaseOrder("REFUND", message, purchaseOrder.CustomerId, purchaseOrder);

            _orderRepository.Save(purchaseOrder);

            return PaymentProcessingResult.CreateSuccessfulResult(message);
        }

        #endregion


        #region Handle OpenpayPortal callback

        /// <summary>
        /// Processes the successful transaction.
        /// </summary>
        /// <param name="currentCart">Current cart.</param>
        /// <param name="payment">Payment.</param>
        /// <param name="configuration">Payment.</param>
        /// <param name="acceptUrl">The accept url. </param>
        /// <param name="cancelUrl">The cancel url.</param>
        /// <returns>The url redirection after process.</returns>
        public string ProcessSuccessfulTransaction(ICart currentCart, IPayment payment, OpenpayConfiguration configuration, string acceptUrl, string cancelUrl)
        {
            if (HttpContext.Current == null)
            {
                return cancelUrl;
            }

            var redirectionUrl = cancelUrl;
            var openpayOrderId = payment.Properties[OpenpayConfigurationConstants.OpenpayOrderId] as string;
            var merchantOrderId = payment.Properties[OpenpayConfigurationConstants.MerchantOrderId] as string;

            #region GetOrderStatus Call
            // Get Openpay purchase price by order status call
            Order orderStatus = null;
            var timeoutException = false;
            try
            {
                orderStatus = OpenpayApiHelper.GetOrderStatusApi(openpayOrderId, configuration, timeoutRetry: true);
            }
            catch (Exception e)
            {
                _logger.Error($"{e.Message}. {e.InnerException}");
                // If Timeout exception occur then continue to process the order
                if (e is WebException webException && webException.Status == WebExceptionStatus.Timeout)
                {
                    timeoutException = true;
                }
                else
                {
                    return ProcessUnsuccessfulTransaction(cancelUrl, Utilities.Translate("GetOrderStatusApiError"));
                }
            }

            #endregion

            #region Compare purchase price, create order & call CaptureOrderPayment

            var totalsInCent = Utilities.ToOpenpayPrice(payment.Amount);
            if ((orderStatus == null || orderStatus.PurchasePrice != totalsInCent) && !timeoutException)
            {
                return ProcessUnsuccessfulTransaction(cancelUrl, Utilities.Translate("PlanPurchasePriceCompareError"));
            }

            try
            {
                // Create Merchant Order in OnHold/Pending status
                var errorMessages = new List<string>();
                var cartCompleted = DoCompletingCart(currentCart, errorMessages);
                if (!cartCompleted)
                {
                    return ProcessUnsuccessfulTransaction(cancelUrl, string.Join("; ", errorMessages.Distinct().ToArray()));
                }
                var purchaseOrder = MakePurchaseOrder(currentCart, merchantOrderId, openpayOrderId);

                // Capture Order Payment Call
                var captureResponse = OpenpayApiHelper.CaptureOrderPayment(purchaseOrder.OrderNumber, openpayOrderId, configuration);
                var captureMessage = string.Empty;

                if (captureResponse != null)
                {
                    DoCaptureTransaction(purchaseOrder, payment, captureResponse, payment.Amount, openpayOrderId, ref captureMessage);
                    redirectionUrl = CreateRedirectionUrl(purchaseOrder, acceptUrl, "success", payment.BillingAddress.Email, captureMessage);
                    _logger.Information($"Openpay transaction succeeds, redirect end user to {redirectionUrl}");
                }
                else
                {
                    Utilities.AddNoteToPurchaseOrder("CAPTURE FAILURE", "Order have been pending since we cannot capture the payment", purchaseOrder.CustomerId, purchaseOrder);
                    captureMessage = "Your order have been pending since we cannot capture the payment with Openpay Payment. " +
                                     "The order will be cancelled in 30 minutes. Please contact site's administrator for further information !";
                    redirectionUrl = CreateRedirectionUrl(purchaseOrder, acceptUrl, "pending", payment.BillingAddress.Email, captureMessage);
                    _logger.Information($"Openpay transaction succeeds, redirect end user to {redirectionUrl}");
                }
            }
            catch (Exception e)
            {
                _logger.Error($"{e.Message}. {e.InnerException}");
                return ProcessUnsuccessfulTransaction(cancelUrl, Utilities.Translate("PaymentCaptureError"));
            }

            #endregion

            return redirectionUrl;
        }


        /// <summary>
        /// Processes the unsuccessful transaction.
        /// </summary>
        /// <param name="cancelUrl">The cancel url.</param>
        /// <param name="errorMessage">The error message.</param>
        /// <returns>The url redirection after process.</returns>
        public string ProcessUnsuccessfulTransaction(string cancelUrl, string errorMessage)
        {
            if (HttpContext.Current == null)
            {
                return cancelUrl;
            }

            _logger.Error($"Openpay transaction failed [{errorMessage}].");
            return UriUtil.AddQueryString(cancelUrl, "message", HttpUtility.UrlEncode(errorMessage));
        }

        #endregion


        #region private helper methods

        private IPurchaseOrder MakePurchaseOrder(ICart cart, string merchantOrderId, string openpayOrderId)
        {
            var orderReference = _orderRepository.SaveAsPurchaseOrder(cart);
            var purchaseOrder = _orderRepository.Load<IPurchaseOrder>(orderReference.OrderGroupId);
            purchaseOrder.OrderNumber = merchantOrderId;
            purchaseOrder.Properties[OpenpayConfigurationConstants.OpenpayOrderId] = openpayOrderId;
            UpdateTransactionIdOfPaymentMethod(purchaseOrder, openpayOrderId);

            if (_databaseMode.Value != DatabaseMode.ReadOnly)
            {
                // Update last order date time for CurrentContact
                UpdateLastOrderTimestampOfCurrentContact(CustomerContext.Current.CurrentContact, purchaseOrder.Created);
            }

            Utilities.AddNoteToPurchaseOrder(string.Empty, $"New order placed by {PrincipalInfo.CurrentPrincipal.Identity.Name} in Public site", Guid.Empty, purchaseOrder);

            // Set Status to OnHold = Pending in Openpay
            purchaseOrder.OrderStatus = OrderStatus.OnHold;

            // Update display name of product by current language
            Utilities.UpdateDisplayNameWithCurrentLanguage(purchaseOrder);

            // Remove old cart
            _orderRepository.Delete(cart.OrderLink);

            _orderRepository.Save(purchaseOrder);

            return purchaseOrder;
        }

        /// <summary>
        /// Validates and completes a cart.
        /// </summary>
        /// <param name="cart">The cart.</param>
        /// <param name="errorMessages">The error messages.</param>
        private bool DoCompletingCart(ICart cart, IList<string> errorMessages)
        {
            // Change status of payments to processed. 
            // It must be done before execute workflow to ensure payments which should mark as processed.
            // To avoid get errors when executed workflow.
            foreach (IPayment p in cart.Forms.SelectMany(f => f.Payments).Where(p => p != null))
            {
                PaymentStatusManager.ProcessPayment(p);
            }

            var isSuccess = true;

            if (_databaseMode.Value != DatabaseMode.ReadOnly)
            {
                if (_featureSwitch.IsSerializedCartsEnabled())
                {
                    var validationIssues = new Dictionary<ILineItem, IList<ValidationIssue>>();
                    cart.AdjustInventoryOrRemoveLineItems(
                        (item, issue) => AddValidationIssues(validationIssues, item, issue), _inventoryProcessor);

                    isSuccess = !validationIssues.Any();

                    foreach (var issue in validationIssues.Values.SelectMany(x => x).Distinct())
                    {
                        if (issue == ValidationIssue.RejectedInventoryRequestDueToInsufficientQuantity)
                        {
                            errorMessages.Add(Utilities.Translate("NotEnoughStockWarning"));
                        }
                        else
                        {
                            errorMessages.Add(Utilities.Translate("CartValidationWarning"));
                        }
                    }

                    return isSuccess;
                }

                // Execute CheckOutWorkflow with parameter to ignore running process payment activity again.
                var isIgnoreProcessPayment = new Dictionary<string, object> { { "PreventProcessPayment", true } };
                var workflowResults = OrderGroupWorkflowManager.RunWorkflow((OrderGroup)cart,
                    OrderGroupWorkflowManager.CartCheckOutWorkflowName, true, isIgnoreProcessPayment);

                var warnings = workflowResults.OutputParameters["Warnings"] as StringDictionary;
                isSuccess = warnings.Count == 0;

                foreach (string message in warnings.Values)
                {
                    errorMessages.Add(message);
                }
            }

            return isSuccess;
        }

        private void DoCaptureTransaction(
            IPurchaseOrder purchaseOrder,
            IPayment payment,
            CapturePaymentResponse captureResponse,
            decimal total,
            string openpayOrderId,
            ref string captureMessage)
        {
            // Update status of purchase order to Processing/Active
            purchaseOrder.OrderStatus = OrderStatus.InProgress;

            var message = $"[{payment.PaymentMethodName}] [Capture payment-{captureResponse.OrderId}] [Status: {OrderStatus.InProgress}] " +
                          $". Timestamp={DateTime.Now}: CapturePrice={captureResponse.PurchasePrice}";
            Utilities.AddNoteToPurchaseOrder("CAPTURE", message, purchaseOrder.CustomerId, purchaseOrder);

            // add new capture payment to order
            var capturePayment = purchaseOrder.CreatePayment(_orderGroupFactory, typeof(OpenpayPayment));
            if (capturePayment != null)
            {
                capturePayment.TransactionType = TransactionType.Capture.ToString();
                capturePayment.BillingAddress = payment.BillingAddress;
                capturePayment.TransactionID = openpayOrderId;
                capturePayment.ProviderTransactionID = openpayOrderId;
                capturePayment.Amount = total;
                capturePayment.PaymentMethodId = _openpayConfiguration.PaymentMethodId;
                capturePayment.PaymentMethodName = OpenpayConfigurationConstants.OpenpaySystemName;
                capturePayment.Status = PaymentStatus.Processed.ToString();
                try
                {
                    purchaseOrder.AddPayment(capturePayment, _orderGroupFactory);
                }
                catch (Exception e)
                {
                    var captureTransaction = purchaseOrder.Forms
                        .SelectMany(x => x.Payments)
                        .FirstOrDefault(y =>
                            y.PaymentMethodId == _openpayConfiguration.PaymentMethodId
                            && y.TransactionType == TransactionType.Capture.ToString());
                    if (captureTransaction == null)
                    {
                        captureMessage = $"The order \"{purchaseOrder.OrderNumber}\" has been captured in Openpay " +
                                         "but cannot create capture transaction in epi commerce manager order.";
                        _logger.Information(captureMessage);
                    }
                }
                _orderRepository.Save(purchaseOrder);
                PaymentStatusManager.ProcessPayment(capturePayment);
            }

        }

        /// <summary>
        /// Update last order time stamp which current user completed.
        /// </summary>
        /// <param name="contact">The customer contact.</param>
        /// <param name="datetime">The order time.</param>
        private void UpdateLastOrderTimestampOfCurrentContact(CustomerContact contact, DateTime datetime)
        {
            if (contact != null)
            {
                contact.LastOrder = datetime;
                contact.SaveChanges();
            }
        }

        private static DatabaseMode GetDefaultDatabaseMode()
        {
            if (!_databaseMode.IsValueCreated)
            {
                return ServiceLocator.Current.GetInstance<IDatabaseMode>().DatabaseMode;
            }
            return _databaseMode.Value;
        }

        private void AddValidationIssues(IDictionary<ILineItem, IList<ValidationIssue>> issues, ILineItem lineItem, ValidationIssue issue)
        {
            if (!issues.ContainsKey(lineItem))
            {
                issues.Add(lineItem, new List<ValidationIssue>());
            }

            if (!issues[lineItem].Contains(issue))
            {
                issues[lineItem].Add(issue);
            }
        }

        private string CreateRedirectionUrl(IPurchaseOrder purchaseOrder, string acceptUrl, string success, string email, string message = "")
        {
            var redirectionUrl = UriUtil.AddQueryString(acceptUrl, "success", success);
            redirectionUrl = UriUtil.AddQueryString(redirectionUrl, "contactId", purchaseOrder.CustomerId.ToString());
            redirectionUrl = UriUtil.AddQueryString(redirectionUrl, "orderNumber", purchaseOrder.OrderLink.OrderGroupId.ToString());
            redirectionUrl = UriUtil.AddQueryString(redirectionUrl, "notificationMessage", message);
            redirectionUrl = UriUtil.AddQueryString(redirectionUrl, "email", email);

            return redirectionUrl;
        }

        private bool ValidateCustomerInfo(IOrderAddress billingAddress, IOrderAddress deliveryAddress)
        {
            if (
                billingAddress == null ||
                string.IsNullOrWhiteSpace(billingAddress.FirstName) ||
                string.IsNullOrWhiteSpace(billingAddress.LastName) ||
                string.IsNullOrWhiteSpace(billingAddress.Email) ||
                string.IsNullOrWhiteSpace(billingAddress.Line1) ||
                string.IsNullOrWhiteSpace(billingAddress.City) ||
                string.IsNullOrWhiteSpace(billingAddress.RegionCode) ||
                string.IsNullOrWhiteSpace(billingAddress.PostalCode)
            )
            {
                return false;
            }

            if (
                deliveryAddress != null &&
                (string.IsNullOrWhiteSpace(deliveryAddress.Line1) ||
                string.IsNullOrWhiteSpace(deliveryAddress.City) ||
                string.IsNullOrWhiteSpace(deliveryAddress.RegionCode) ||
                string.IsNullOrWhiteSpace(deliveryAddress.PostalCode))
            )
            {
                return false;
            }

            return true;
        }

        /// <summary>
        /// Loop through all payments in the PurchaseOrder, find payment of paymentMethod type, set TransactionId.
        /// </summary>
        /// <param name="purchaseOrder">The purchase order.</param>
        /// <param name="paymentGatewayTransactionID">The transactionId from Openpay.</param>
        private void UpdateTransactionIdOfPaymentMethod(IPurchaseOrder purchaseOrder, string paymentGatewayTransactionID)
        {
            // loop through all payments in the PurchaseOrder, find payment with id equal guidPaymentMethodId, set TransactionId
            foreach (var payment in purchaseOrder.Forms.SelectMany(form => form.Payments).Where(payment => payment.PaymentMethodId.Equals(_openpayConfiguration.PaymentMethodId)))
            {
                payment.TransactionID = paymentGatewayTransactionID;
                payment.ProviderTransactionID = paymentGatewayTransactionID;
            }
        }

        private PaymentProcessingResult RefundError(string logMessage, IPurchaseOrder purchaseOrder)
        {
            Utilities.AddNoteToPurchaseOrder("REFUND", logMessage, purchaseOrder.CustomerId, purchaseOrder);
            _orderRepository.Save(purchaseOrder);
            _logger.Error(logMessage);
            return PaymentProcessingResult.CreateUnsuccessfulResult(logMessage);
        }

        #endregion

    }
}
