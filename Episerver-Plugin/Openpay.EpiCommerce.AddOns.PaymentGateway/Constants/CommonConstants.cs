using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Openpay.EpiCommerce.AddOns.PaymentGateway.Constants
{
    public static class CommonConstants
    {
        public const string OpenpayLogAppender = "OpenpayLogAppender";
        public const string PlanCreationTypeValue = "Pending";
        public const string EpiserverBrandName = "Optimizely|Episerver CMS";
        public const string OpenpayProductPriceValue = "OpenpayProductPriceValue";

        public const string OpenpayGetPurchaseLimitsApiEndPoint = "/orders/limits";
        public const string OpenpayRefundApiEndPoint = "/orders/{0}/refund";
        public const string OpenpayGetOrderByIdApiEndPoint = "/orders/";
        public const string OpenpayCaptureOrderPayment = "/orders/{0}/capturepayment";

        public static class ResponseFormFields
        {
            public const string JamPlanID = "JamPlanID";
            public const string TransactionToken = "TransactionToken";
        }

        public static class OpenpayTabNames
        {
            public const string OpenpayPage = "Openpay Payment";
            public const string OpenpayWidgetBlock = "Openpay Widget Blocks";
            public const string InfoBeltWidgetConfiguration = "Info Belt Widget";
            public const string ProductListingWidgetConfiguration = "Product Listing Widget";
            public const string GeneralConfiguration = "General Configuration";
            public const string CartWidgetConfiguration = "Cart Widget";
            public const string ProductPageWidgetConfiguration = "ProductPage Widget";
            public const string LearnMorePopupConfiguration = "Learn More Popup";
            public const string CheckoutWidgetConfiguration = "Checkout Page Widget";
        }
    }
}
