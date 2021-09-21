using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.UI.WebControls;
using Openpay.EpiCommerce.AddOns.PaymentGateway.Enums;
using Openpay.EpiCommerce.AddOns.PaymentGateway.Helpers;

namespace Openpay.EpiCommerce.AddOns.PaymentGateway.Constants
{
    public static class OpenpayConfigurationConstants
    {
        public static class AUConfiguration
        {
            public static class Sandbox
            {
                public const string ApiUrl = "https://api.training.myopenpay.com.au/v1/merchant";
                public const string HandoverUrl = "https://retailer.myopenpay.com.au/websalestraining/?TransactionToken=";
            }

            public static class Production
            {
                public const string ApiUrl = "https://api.myopenpay.com.au/v1/merchant";
                public const string HandoverUrl = "https://retailer.myopenpay.com.au/websaleslive/?TransactionToken=";
            }
        }

        public static class UKConfiguration
        {
            public static class Sandbox
            {
                public const string ApiUrl = "https://api.training.myopenpay.co.uk/v1/merchant";
                public const string HandoverUrl = "https://websales.training.myopenpay.co.uk/?TransactionToken=";
            }

            public static class Production
            {
                public const string ApiUrl = "https://api.myopenpay.co.uk/v1/merchant";
                public const string HandoverUrl = "https://websales.myopenpay.co.uk/?TransactionToken=";
            }
        }

        public static class USAConfiguration
        {
            public static class Sandbox
            {
                public const string ApiUrl = "https://api.training.myopenpay.com/v1/merchant";
                public const string HandoverUrl = "https://websales.training.myopenpay.com/?TransactionToken=";
            }

            public static class Production
            {
                public const string ApiUrl = "https://api.myopenpay.com/v1/merchant";
                public const string HandoverUrl = "https://websales.myopenpay.com/?TransactionToken=";
            }
        }

        public const string OpenpaySystemName = "Openpay";
        public const string RegionParam = "Region";
        public const string DescriptionParam = "Description";
        public const string EnvironmentParam = "Environment";
        public const string Username = "Username";
        public const string Password = "Password";
        public const string ExcludedProductConfigItem = "ExcludedProductConfigItem";
        public const string MinPurchaseLimitParam = "MinPurchaseLimit";
        public const string MaxPurchaseLimitParam = "MaxPurchaseLimit";
        public const string CallBackItemId = "CallBackItemId";
        public const string OpenpayOrderId = "OpenpayOrderId";
        public const string MerchantOrderId = "MerchantOrderId";
        public const string WidgetConfigurationBlock = "OpenpayWidgetConfigurationBlock";

        public static List<ListItem> RegionListItems => new List<ListItem>
        {
            new ListItem(OpenpayRegion.AU.GetDescription(), OpenpayRegion.AU.ToString()),
            new ListItem(OpenpayRegion.USA.GetDescription(), OpenpayRegion.USA.ToString()),
            new ListItem(OpenpayRegion.UK.GetDescription(), OpenpayRegion.UK.ToString()),
        };

        public static List<ListItem> EnvironmentListItems => new List<ListItem>
        {
            new ListItem(OpenpayEnvironment.Sandbox.ToString(), OpenpayEnvironment.Sandbox.ToString()),
            new ListItem(OpenpayEnvironment.Production.ToString(), OpenpayEnvironment.Production.ToString())
        };
    }
}
