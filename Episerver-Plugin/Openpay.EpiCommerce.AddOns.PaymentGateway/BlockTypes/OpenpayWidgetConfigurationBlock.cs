using EPiServer.Core;
using EPiServer.DataAbstraction;
using EPiServer.DataAnnotations;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using EPiServer.ServiceLocation;
using EPiServer.Shell.ObjectEditing;
using EPiServer.Web;
using Mediachase.Commerce;
using Openpay.EpiCommerce.AddOns.PaymentGateway.Constants;
using Openpay.EpiCommerce.AddOns.PaymentGateway.Enums;
using Openpay.EpiCommerce.AddOns.PaymentGateway.Helpers;
using Openpay.EpiCommerce.AddOns.PaymentGateway.Models.Widget;
using Openpay.EpiCommerce.AddOns.PaymentGateway.Models.Widget.SelectionFactory;

namespace Openpay.EpiCommerce.AddOns.PaymentGateway.BlockTypes
{
    [ContentType(
        DisplayName = "Openpay Widget Configuration Block", 
        GUID = "45942f47-373c-465b-b7e5-bb40eba97875", 
        Description = "This block contains the configuration script to render Openpay Widget.",
        GroupName = CommonConstants.OpenpayTabNames.OpenpayWidgetBlock)]
    [ImageUrl("~/styles/images/Openpay-apple-touch-icon.png")]
    public class OpenpayWidgetConfigurationBlock : BlockData
    {

        #region General Configuration

        [CultureSpecific]
        [Display(
            Name = "Region",
            Description = "The region your store is located in.",
            GroupName = CommonConstants.OpenpayTabNames.GeneralConfiguration,
            Order = 1)]
        [Required]
        [SelectOne(SelectionFactoryType = typeof(OpenpayRegionSelectionFactory))]
        public virtual string Region { get; set; }

        [CultureSpecific]
        [Display(
            Name = "Currency",
            Description = "The currency symbol used in the widgets. Its value can be : $‎, A$, AU$, AUD or £",
            GroupName = CommonConstants.OpenpayTabNames.GeneralConfiguration,
            Order = 2)]
        public virtual string Currency { get; set; }

        [CultureSpecific]
        [Display(
            Name = "Plan Tiers",
            Description = "An array of available plan tiers you have available in months. E.g. [2, 4, 6] for 2 months, 4 months and 6 months",
            GroupName = CommonConstants.OpenpayTabNames.GeneralConfiguration,
            Order = 3)]
        [Required]
        public virtual IList<int> PlanTiers { get; set; }

        [CultureSpecific]
        [Display(
            Name = "Min Eligible Amount",
            Description = "The minimum eligible amount required before Openpay is eligible e.g 50",
            GroupName = CommonConstants.OpenpayTabNames.GeneralConfiguration,
            Order = 4)]
        [ReadOnly(true)]
        public virtual decimal MinEligibleAmount { get; set; }

        [CultureSpecific]
        [Display(
            Name = "Max Eligible Amount",
            Description = "The maximum eligible amount required before Openpay is eligible e.g 1000",
            GroupName = CommonConstants.OpenpayTabNames.GeneralConfiguration,
            Order = 5)]
        [ReadOnly(true)]
        public virtual decimal MaxEligibleAmount { get; set; }

        [CultureSpecific]
        [Display(
            Name = "Type",
            Description = "Type of your store",
            GroupName = CommonConstants.OpenpayTabNames.GeneralConfiguration,
            Order = 6)]
        [SelectOne(SelectionFactoryType = typeof(OpenpayStoreTypeSelectionFactory))]
        [ReadOnly(true)]
        public virtual string Type { get; set; }

        [CultureSpecific]
        [Display(
            Name = "Customize Wrapper Css",
            Description = "Customize css that apply to the div wrapper of the widget",
            GroupName = CommonConstants.OpenpayTabNames.GeneralConfiguration,
            Order = 2)]
        [UIHint(UIHint.Textarea)]
        public virtual string WrapperCss { get; set; }

        #endregion


        #region InfoBelt Widget

        [CultureSpecific]
        [Display(
            Name = "Show Info Belt Widget",
            Description = "Show the widget on home page or across all page",
            GroupName = CommonConstants.OpenpayTabNames.InfoBeltWidgetConfiguration,
            Order = 1)]
        [SelectOne(SelectionFactoryType = typeof(ShowInfoBeltSelectionFactory))]
        public virtual string ShowInfoBeltWidget { get; set; }

        [CultureSpecific]
        [Display(
            Name = "Info Belt Color",
            Description = "Openpay Logo to use that suits your website background",
            GroupName = CommonConstants.OpenpayTabNames.InfoBeltWidgetConfiguration,
            Order = 1)]
        [SelectOne(SelectionFactoryType = typeof(InfoBeltLogoSelectionFactory))]
        public virtual string InfoBeltLogo { get; set; }

        [CultureSpecific]
        [Display(
            Name = "Sticky Info Belt Widget",
            Description = "Make the Info belt widget sticky on the website pages where the header position is fixed",
            GroupName = CommonConstants.OpenpayTabNames.InfoBeltWidgetConfiguration,
            Order = 1)]
        public virtual bool StickyInfoBeltWidget { get; set; }

        [CultureSpecific]
        [Display(
            Name = "Customize Sticky Script",
            Description = "Script to make InfoBelt widget sticky",
            GroupName = CommonConstants.OpenpayTabNames.InfoBeltWidgetConfiguration,
            Order = 1)]
        [UIHint(UIHint.Textarea)]
        public virtual string StickyScript { get; set; }

        #endregion


        #region ProductPage Widget

        [CultureSpecific]
        [Display(
            Name = "Product Page Widget Logo",
            Description = "Openpay Logo to use that suits your website background",
            GroupName = CommonConstants.OpenpayTabNames.ProductPageWidgetConfiguration,
            Order = 1)]
        [SelectOne(SelectionFactoryType = typeof(ProductPageLogoSelectionFactory))]
        public virtual string ProductPageLogo { get; set; }

        [CultureSpecific]
        [Display(
            Name = "Product Page Logo Position",
            Description = "Openpay Logo position to use that suits your website",
            GroupName = CommonConstants.OpenpayTabNames.ProductPageWidgetConfiguration,
            Order = 1)]
        [SelectOne(SelectionFactoryType = typeof(ProductPageLogoPositionSelectionFactory))]
        public virtual string ProductPageLogoPosition { get; set; }

        [CultureSpecific]
        [Display(
            Name = "Product Page Learnmore text",
            GroupName = CommonConstants.OpenpayTabNames.ProductPageWidgetConfiguration,
            Order = 1)]
        public virtual string ProductPageLearnMoreText { get; set; }

        [CultureSpecific]
        [Display(
            Name = "Show Product Page Widget",
            GroupName = CommonConstants.OpenpayTabNames.ProductPageWidgetConfiguration,
            Order = 1)]
        public virtual bool ShowProductPageWidget { get; set; }

        [CultureSpecific]
        [Display(
            Name = "Product Page Info Icon",
            GroupName = CommonConstants.OpenpayTabNames.ProductPageWidgetConfiguration,
            Order = 1)]
        [SelectOne(SelectionFactoryType = typeof(InfoIconSelectionFactory))]
        public virtual string ProductPageInfoIcon { get; set; }

        #endregion


        #region ProductListing Widget

        [CultureSpecific]
        [Display(
            Name = "Listing Page Widget Logo",
            GroupName = CommonConstants.OpenpayTabNames.ProductListingWidgetConfiguration,
            Order = 1)]
        [SelectOne(SelectionFactoryType = typeof(ProductListingLogoSelectionFactory))]
        public virtual string ProductListingWidgetLogo { get; set; }

        [CultureSpecific]
        [Display(
            Name = "Hide Logo",
            GroupName = CommonConstants.OpenpayTabNames.ProductListingWidgetConfiguration,
            Order = 2)]
        public virtual bool ProductListingHideLogo { get; set; }

        [CultureSpecific]
        [Display(
            Name = "Show Product Listing Page Widget",
            GroupName = CommonConstants.OpenpayTabNames.ProductListingWidgetConfiguration,
            Order = 3)]
        public virtual bool ShowProductListingWidget { get; set; }

        #endregion


        #region Cart Widget

        [CultureSpecific]
        [Display(
            Name = "Cart Widget Logo",
            Description = "Openpay Logo to use that suits your website background",
            GroupName = CommonConstants.OpenpayTabNames.CartWidgetConfiguration,
            Order = 1)]
        [SelectOne(SelectionFactoryType = typeof(CartLogoSelectionFactory))]
        public virtual string CartLogo { get; set; }

        [CultureSpecific]
        [Display(
            Name = "Learn More Link Text",
            Description = "Enter any text to display for the clickable link, Default is Learn More",
            GroupName = CommonConstants.OpenpayTabNames.CartWidgetConfiguration,
            Order = 2)]
        public virtual string CartLearnMoreText { get; set; }

        [CultureSpecific]
        [Display(
            Name = "Show Cart Page Widget",
            GroupName = CommonConstants.OpenpayTabNames.CartWidgetConfiguration,
            Order = 3)]
        public virtual bool ShowCartWidget { get; set; }

        [CultureSpecific]
        [Display(
            Name = "Cart Widget Info Icon",
            GroupName = CommonConstants.OpenpayTabNames.CartWidgetConfiguration,
            Order = 1)]
        [SelectOne(SelectionFactoryType = typeof(InfoIconSelectionFactory))]
        public virtual string CartInfoIcon { get; set; }

        #endregion


        #region LearnMore Popup

        [CultureSpecific]
        [Display(
            Name = "Logo Widget",
            GroupName = CommonConstants.OpenpayTabNames.LearnMorePopupConfiguration,
            Order = 1)]
        [SelectOne(SelectionFactoryType = typeof(LogoWidgetSelectionFactory))]
        public virtual string LogoWidget { get; set; }

        [CultureSpecific]
        [Display(
            Name = "Learn More Link Widget",
            GroupName = CommonConstants.OpenpayTabNames.LearnMorePopupConfiguration,
            Order = 1)]
        public virtual string LearnMoreLinkWidgetText { get; set; }

        [CultureSpecific]
        [Display(
            Name = "Learn More Info Icon",
            GroupName = CommonConstants.OpenpayTabNames.LearnMorePopupConfiguration,
            Order = 1)]
        [SelectOne(SelectionFactoryType = typeof(InfoIconSelectionFactory))]
        public virtual string LearnMoreInfoIcon { get; set; }

        #endregion


        #region Checkout Page Widget

        [CultureSpecific]
        [Display(
            Name = "Show Logo Widget",
            GroupName = CommonConstants.OpenpayTabNames.CheckoutWidgetConfiguration,
            Order = 1)]
        public virtual bool ShowLogoWidget { get; set; }

        [CultureSpecific]
        [Display(
            Name = "Show Learn More Link Widget",
            GroupName = CommonConstants.OpenpayTabNames.CheckoutWidgetConfiguration,
            Order = 1)]
        public virtual bool ShowLearnMoreWidget { get; set; }

        [CultureSpecific]
        [Display(
            Name = "Show Checkout Page Widget",
            GroupName = CommonConstants.OpenpayTabNames.CheckoutWidgetConfiguration,
            Order = 1)]
        public virtual bool ShowCheckoutPageWidget { get; set; }

        [CultureSpecific]
        [Display(
            Name = "Instalment Text",
            GroupName = CommonConstants.OpenpayTabNames.CheckoutWidgetConfiguration,
            Order = 1)]
        public virtual XhtmlString InstalmentText { get; set; }

        #endregion

        public override void SetDefaultValues(ContentType contentType)
        {
            base.SetDefaultValues(contentType);
            // Set eligible amount
            var openpayConfiguration = new OpenpayConfiguration();
            decimal.TryParse(openpayConfiguration.MinPurchaseLimit, out var minPrice);
            decimal.TryParse(openpayConfiguration.MaxPurchaseLimit, out var maxPrice);
            MinEligibleAmount = minPrice;
            MaxEligibleAmount = maxPrice;

            // Set currency
            var currentMarket = ServiceLocator.Current.GetInstance<ICurrentMarket>();
            var currency = currentMarket.GetCurrentMarket().DefaultCurrency;
            Currency = currency.Format.CurrencySymbol;

            // Set store type
            Type = OrderOrigin.Online.ToString();
            ShowInfoBeltWidget = ShowInfoBeltOption.No.ToString();

            // Set show widget properties
            ShowProductPageWidget = true;
            ShowProductListingWidget = true;
            ShowCartWidget = true;
            ShowLearnMoreWidget = true;
            ShowLogoWidget = true;
            ShowCheckoutPageWidget = true;

            ShowCartWidget = true;
            
            // Set Info belt
            InfoBeltLogo = "white";
            StickyInfoBeltWidget = false;
            StickyScript = @"
                window.onscroll = function() {opyinfobelt()};
                var optopbar = document.getElementById(""openpayinfobelt"");
                var optopbarbottom = document.getElementById(""openpaybottom"");
                var sticky = optopbar.offsetTop;
                function opyinfobelt() {  
                    if (window.pageYOffset >= sticky) {    optopbar.classList.add(""openpaysticky"")    optopbarbottom.classList.add(""openpaybottom"")  } 
                    else {    optopbar.classList.remove(""openpaysticky"");    optopbarbottom.classList.add(""openpaybottom"")  }
                }
                ";

            // product page widget
            ProductPageLogoPosition = "left";

            // product listing widget
            ProductListingHideLogo = false;

            // Set learn more popup
            LogoWidget = "amber";

            // set info-icon empty by default
            CartInfoIcon = "";
            ProductPageInfoIcon = "";
            LearnMoreInfoIcon = "";
        }
    }
}