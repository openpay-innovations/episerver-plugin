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
using EPiServer.Web.Routing;
using Mediachase.Commerce.Orders;
using Mediachase.Commerce.Orders.Exceptions;
using Mediachase.Commerce.Security;
using Openpay.EpiCommerce.AddOns.PaymentGateway.Constants;
using Openpay.EpiCommerce.AddOns.PaymentGateway.Enums;
using Openpay.EpiCommerce.AddOns.PaymentGateway.Helpers;
using Openpay.EpiCommerce.AddOns.PaymentGateway.PageTypes;

namespace Openpay.EpiCommerce.AddOns.PaymentGateway.Controllers
{
    public class OpenpayPaymentController : PageController<OpenpayPaymentHandlerPage>
    {
        private readonly IOrderRepository _orderRepository;
        private static UrlResolver _urlResolver;

        public OpenpayPaymentController() : this(
            ServiceLocator.Current.GetInstance<IOrderRepository>(),
            ServiceLocator.Current.GetInstance<UrlResolver>()
        )
        { }

        public OpenpayPaymentController(IOrderRepository orderRepository, UrlResolver urlResolver)
        {
            _orderRepository = orderRepository;
            _urlResolver = urlResolver;
        }

        public ActionResult Index()
        {
            if (PageEditing.PageIsInEditMode)
            {
                return new EmptyResult();
            }

            var homePageUrl = _urlResolver.GetUrl(ContentReference.StartPage);

            var currentCart = _orderRepository.LoadCart<ICart>(PrincipalInfo.CurrentPrincipal.GetContactId(), Cart.DefaultName);
            if (currentCart == null || !currentCart.Forms.Any() || !currentCart.GetFirstForm().Payments.Any())
            {
                return Redirect(homePageUrl);
            }

            var openpayConfiguration = new OpenpayConfiguration();
            var payment = currentCart.Forms.SelectMany(f => f.Payments).FirstOrDefault(c => c.PaymentMethodId.Equals(openpayConfiguration.PaymentMethodId));
            if (payment == null)
            {
                return Redirect(homePageUrl);
            }

            var merchantOrderId = payment.Properties[OpenpayConfigurationConstants.MerchantOrderId].ToString();
            var openpayOrderId = payment.Properties[OpenpayConfigurationConstants.OpenpayOrderId] as string;
            var queryStringOrderId = Request.QueryString["orderid"];
            if (string.IsNullOrWhiteSpace(merchantOrderId) || string.IsNullOrWhiteSpace(openpayOrderId) || string.IsNullOrWhiteSpace(queryStringOrderId) || merchantOrderId != queryStringOrderId)
            {
                return Redirect(homePageUrl);
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