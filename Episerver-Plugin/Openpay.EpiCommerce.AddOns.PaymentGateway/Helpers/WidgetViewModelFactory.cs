using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using EPiServer;
using EPiServer.Commerce.Catalog.ContentTypes;
using EPiServer.Commerce.Order;
using EPiServer.Core;
using EPiServer.Security;
using EPiServer.ServiceLocation;
using EPiServer.Web.Routing;
using Mediachase.Commerce.Catalog;
using Mediachase.Commerce.Orders;
using Mediachase.Commerce.Security;
using Openpay.EpiCommerce.AddOns.PaymentGateway.BlockTypes;
using Openpay.EpiCommerce.AddOns.PaymentGateway.Enums;
using Openpay.EpiCommerce.AddOns.PaymentGateway.Models.Widget;

namespace Openpay.EpiCommerce.AddOns.PaymentGateway.Helpers
{
    public static class WidgetViewModelFactory
    {

        public static InfoBeltWidgetViewModel GetInfoBeltWidgetViewModel()
        {
            var contentRouteHelper = ServiceLocator.Current.GetInstance<IContentRouteHelper>();
            var configBlock = GetConfigurationBlock();

            if (configBlock == null || contentRouteHelper.ContentLink == null || ContentReference.StartPage == null || !ValidateCurrentContentItem())
            {
                return null;
            }
            var currentContentId = contentRouteHelper.ContentLink.ID;
            var startPageId = ContentReference.StartPage.ID;

            bool showWidget = configBlock.ShowInfoBeltWidget == ShowInfoBeltOption.AllPage.ToString() || (configBlock.ShowInfoBeltWidget == ShowInfoBeltOption.HomePage.ToString() && currentContentId == startPageId);
            var model = new InfoBeltWidgetViewModel
            {
                ShowWidget = showWidget,
                Logo = configBlock.InfoBeltLogo,
                StickyInfoBeltWidget = configBlock.StickyInfoBeltWidget,
                StickyScript = configBlock.StickyScript,
                WrapperCss = configBlock.WrapperCss
            };
            return model;
        }

        public static ProductPageWidgetViewModel GetProductPageWidgetViewModel(decimal productPrice)
        {
            var configBlock = GetConfigurationBlock();
            if (productPrice <= 0 || configBlock == null || !ValidateCurrentContentItem())
            {
                return null;
            }

            var viewModel = new ProductPageWidgetViewModel
            {
                ProductPrice = string.Format("{0:0.00}", productPrice),
                ProductPageLogo = configBlock.ProductPageLogo,
                ProductPageLearnMoreText = configBlock.ProductPageLearnMoreText,
                ProductPageLogoPosition = configBlock.ProductPageLogoPosition,
                ShowProductPageWidget = configBlock.ShowProductPageWidget,
                InfoIcon = configBlock.ProductPageInfoIcon,
                WrapperCss = configBlock.WrapperCss
            };

            return viewModel;
        }

        public static ProductListingWidgetViewModel GetProductListingWidgetViewModel(decimal productPrice, string contentCode)
        {
            var configBlock = GetConfigurationBlock();
            if (productPrice <= 0 || configBlock == null || !ValidateCurrentContentItem(contentCode))
            {
                return null;
            }

            var model = new ProductListingWidgetViewModel
            {
                ProductPrice = productPrice,
                ShowWidget = configBlock.ShowProductListingWidget,
                HideLogo = configBlock.ProductListingHideLogo.ToString().ToLower(),
                ListingPageWidgetLogo = configBlock.ProductListingWidgetLogo,
                WrapperCss = configBlock.WrapperCss
            };

            return model;
        }

        public static CartWidgetViewModel GetCartWidgetViewModel()
        {
            var configBlock = GetConfigurationBlock();
            var orderRepository = ServiceLocator.Current.GetInstance<IOrderRepository>();
            var currentCart = orderRepository.LoadCart<ICart>(PrincipalInfo.CurrentPrincipal.GetContactId(), Cart.DefaultName);
            if (configBlock == null || currentCart == null || !Utilities.ValidateProhibitedProducts(currentCart, new OpenpayConfiguration().ExcludedProductConfigItem))
            {
                return null;
            }
            var cartTotal = currentCart.GetTotal();
            var model = new CartWidgetViewModel
            {
                CartTotal = cartTotal,
                LearnMoreText = configBlock.CartLearnMoreText,
                Logo = configBlock.CartLogo,
                ShowCartWidget = configBlock.ShowCartWidget,
                InfoIcon = configBlock.CartInfoIcon,
                WrapperCss = configBlock.WrapperCss
            };
            return model;
        }

        public static LearnMoreLinkWidgetViewModel GetLearnMoreLinkWidgetViewModel()
        {
            var configBlock = GetConfigurationBlock();
            if (configBlock == null || !ValidateCurrentContentItem())
            {
                return null;
            }
            var viewModel = new LearnMoreLinkWidgetViewModel
            {
                LearnMoreLinkText = configBlock.LearnMoreLinkWidgetText,
                LearnMoreInfoIcon = configBlock.LearnMoreInfoIcon,
                WrapperCss = configBlock.WrapperCss
            };

            return viewModel;
        }

        public static LogoWidgetViewModel GetLogoWidgetViewModel()
        {
            var configBlock = GetConfigurationBlock();
            if (configBlock == null || !ValidateCurrentContentItem())
            {
                return null;
            }
            var viewModel = new LogoWidgetViewModel
            {
                Logo = configBlock.LogoWidget,
                WrapperCss = configBlock.WrapperCss
            };

            return viewModel;
        }

        public static CheckoutWidgetViewModel GetCheckoutWidgetViewModel()
        {
            var configBlock = GetConfigurationBlock();
            if (configBlock == null || !ValidateCurrentContentItem())
            {
                return null;
            }
            var viewModel = new CheckoutWidgetViewModel
            {
                ShowCheckoutWidget = configBlock.ShowCheckoutPageWidget,
                ShowLearnMoreWidget = configBlock.ShowLearnMoreWidget,
                ShowLogoWidget = configBlock.ShowLogoWidget,
                InstalmentText = configBlock.InstalmentText,
                LearnMore = configBlock.LearnMoreLinkWidgetText,
                Logo = configBlock.LogoWidget,
                InfoIcon = configBlock.LearnMoreInfoIcon,
                WrapperCss = configBlock.WrapperCss
            };

            return viewModel;
        }

        public static OpenpayWidgetConfigurationBlock GetConfigurationBlock()
        {
            var contentLoader = ServiceLocator.Current.GetInstance<IContentLoader>();
            var statPage = contentLoader.Get<PageData>(ContentReference.StartPage);
            var contentLink = statPage.Property[Constants.OpenpayConfigurationConstants.WidgetConfigurationBlock]?.Value as ContentReference;
            if (contentLink == null)
            {
                return null;
            }
            return contentLoader.Get<OpenpayWidgetConfigurationBlock>(contentLink);
        }

        private static bool ValidateCurrentContentItem(string contentCode = "")
        {
            var contentRouteHelper = ServiceLocator.Current.GetInstance<IContentRouteHelper>();
            var contentLoader = ServiceLocator.Current.GetInstance<IContentLoader>();

            if (contentRouteHelper.ContentLink == null && string.IsNullOrWhiteSpace(contentCode))
                return true;

            IContent currentContentItem;
            if (!string.IsNullOrWhiteSpace(contentCode))
            {
                var referenceConverter = ServiceLocator.Current.GetInstance<ReferenceConverter>();
                var contentLink = referenceConverter.GetContentLink(contentCode);

                currentContentItem = contentLoader.Get<IContent>(contentLink);
            }
            else
            {
                currentContentItem = contentLoader.Get<IContent>(contentRouteHelper.ContentLink);
            }

            if (currentContentItem is EntryContentBase currentContent)
            {
                return Utilities.ValidateContentItem(currentContent, new OpenpayConfiguration().ExcludedProductConfigItem);
            }

            return true;
        }

    }
}
