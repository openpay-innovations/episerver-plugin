using System;
using EPiServer;
using EPiServer.Core;
using EPiServer.Framework.DataAnnotations;
using EPiServer.Web.Mvc;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using EPiServer.Commerce.Order;
using EPiServer.ServiceLocation;
using EPiServer.Editor;
using EPiServer.Security;
using EPiServer.Web;
using Mediachase.Commerce.Orders;
using Mediachase.Commerce.Orders.Exceptions;
using Mediachase.Commerce.Security;
using Openpay.EpiCommerce.AddOns.PaymentGateway.Enums;
using Openpay.EpiCommerce.AddOns.PaymentGateway.Helpers;
using Openpay.EpiCommerce.AddOns.PaymentGateway.PageTypes;

namespace Openpay.EpiCommerce.AddOns.PaymentGateway.Controllers
{
    public class OpenpayPaymentController : PageController<OpenpayPaymentHandlerPage>
    {
        private readonly IOrderRepository _orderRepository;

        public OpenpayPaymentController() : this(
            ServiceLocator.Current.GetInstance<IOrderRepository>()
        )
        { }

        public OpenpayPaymentController(IOrderRepository orderRepository)
        {
            _orderRepository = orderRepository;
        }

        public ActionResult Index()
        {
            if (PageEditing.PageIsInEditMode)
            {
                return new EmptyResult();
            }

            var currentCart = _orderRepository.LoadCart<ICart>(PrincipalInfo.CurrentPrincipal.GetContactId(), Cart.DefaultName);
            if (!currentCart.Forms.Any() || !currentCart.GetFirstForm().Payments.Any())
            {
                throw new PaymentException(PaymentException.ErrorType.ProviderError, "", Utilities.Translate("GenericError"));
            }

            var openpayConfiguration = new OpenpayConfiguration();
            var payment = currentCart.Forms.SelectMany(f => f.Payments).FirstOrDefault(c => c.PaymentMethodId.Equals(openpayConfiguration.PaymentMethodId));
            if (payment == null)
            {
                throw new PaymentException(PaymentException.ErrorType.ProviderError, "", Utilities.Translate("PaymentNotSpecified"));
            }

            var openpayOrderId = payment.Properties[Constants.OpenpayConfigurationConstants.OpenpayOrderId] as string;
            if (string.IsNullOrEmpty(openpayOrderId))
            {
                throw new PaymentException(PaymentException.ErrorType.ProviderError, "", Utilities.Translate("PaymentNotSpecified"));
            }

            // Redirect customer to receipt page
            var cancelUrl = Utilities.GetUrlFromOpenpayPaymentPageReferenceProperty(nameof(OpenpayPaymentHandlerPage.OrderCancelPage)); // get link to Checkout page
            cancelUrl = UriUtil.AddQueryString(cancelUrl, "success", bool.FalseString);
            cancelUrl = UriUtil.AddQueryString(cancelUrl, "paymentmethod", Constants.OpenpayConfigurationConstants.OpenpaySystemName);
            var redirectUrl = cancelUrl;

            var gateway = new OpenpayPaymentGateway();
            Enum.TryParse<CreateNewOrderStatus>(Request.QueryString["status"], out var status);

            if (status == CreateNewOrderStatus.LODGED)
            {
                var acceptUrl = Utilities.GetUrlFromOpenpayPaymentPageReferenceProperty(nameof(OpenpayPaymentHandlerPage.OrderCompletePage));
                redirectUrl = gateway.ProcessSuccessfulTransaction(currentCart, payment, openpayConfiguration, acceptUrl, cancelUrl);
            }
            else if (status == CreateNewOrderStatus.CANCELLED)
            {
                TempData["Message"] = Utilities.Translate("CancelMessage");
                redirectUrl = gateway.ProcessUnsuccessfulTransaction(cancelUrl, Utilities.Translate("CancelMessage"));
            }
            else if (status == CreateNewOrderStatus.FAILURE)
            {
                TempData["Message"] = Utilities.Translate("FailureMessage");
                redirectUrl = gateway.ProcessUnsuccessfulTransaction(cancelUrl, Utilities.Translate("FailureMessage"));
            }

            return Redirect(redirectUrl);
        }
    }
}