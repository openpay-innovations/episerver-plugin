using EPiServer.Framework.Localization;
using EPiServer.ServiceLocation;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EPiServer;
using EPiServer.Commerce.Catalog.ContentTypes;
using EPiServer.Commerce.Catalog.Linking;
using EPiServer.Commerce.Order;
using EPiServer.Core;
using EPiServer.Globalization;
using EPiServer.Security;
using EPiServer.Web.Routing;
using Mediachase.Commerce;
using Mediachase.Commerce.Catalog;
using Mediachase.Commerce.Orders;
using Mediachase.Commerce.Security;
using Openpay.EpiCommerce.AddOns.PaymentGateway.Constants;
using Openpay.EpiCommerce.AddOns.PaymentGateway.PageTypes;

namespace Openpay.EpiCommerce.AddOns.PaymentGateway.Helpers
{
    public static class Utilities
    {
        private static Injected<LocalizationService> _localizationService = default(Injected<LocalizationService>);
        private static Injected<IContentLoader> _contentLoader = default(Injected<IContentLoader>);
        private static Injected<UrlResolver> _urlResolver = default(Injected<UrlResolver>);
        private static Injected<ReferenceConverter> _referenceConverter = default(Injected<ReferenceConverter>);
        private static Injected<LanguageResolver> _languageResolver = default(Injected<LanguageResolver>);
        private static Injected<IRelationRepository> _relationRepository = default(Injected<IRelationRepository>);

        private static List<NodeContentBase> _excludeNodeList;
        private static List<EntryContentBase> _excludeEntryList;
        private static CultureInfo _preferredLang = _languageResolver.Service.GetPreferredCulture();

        /// <summary>
        /// Translates with languageKey under /Commerce/Checkout/Openpay/ in lang.xml
        /// </summary>
        /// <param name="languageKey">The language key.</param>
        public static string Translate(string languageKey)
        {
            return GetLocalizationMessage("/Commerce/Checkout/Openpay/" + languageKey);
        }

        /// <summary>
        /// Gets localized message.
        /// </summary>
        /// <param name="path">The path of the message in lang.xml file.</param>
        /// <returns></returns>
        public static string GetLocalizationMessage(string path)
        {
            return _localizationService.Service.GetString(path);
        }

        /// <summary>
        /// Convert decimal price to Openpay price format
        /// </summary>
        /// <param name="price"></param>
        /// <returns></returns>
        public static int ToOpenpayPrice(decimal price)
        {
            if (price <= 0)
            {
                return 0;
            }
            
            var priceAtLowestDenomination = Math.Round(price, 2) * 100;
            var result = decimal.ToInt32(priceAtLowestDenomination);

            return result;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="propertyName"></param>
        /// <returns></returns>
        public static string GetUrlFromOpenpayPaymentPageReferenceProperty(string propertyName)
        {
            var openpayConfiguration = new OpenpayConfiguration();

            if (!string.IsNullOrWhiteSpace(openpayConfiguration.CallbackItemId))
            {
                var callbackItemPageRef = new PageReference(openpayConfiguration.CallbackItemId);
                var openpayPaymentCallbackPage = _contentLoader.Service.Get<OpenpayPaymentHandlerPage>(callbackItemPageRef);

                var contentLink = openpayPaymentCallbackPage?.Property[propertyName]?.Value as ContentReference;
                if (!ContentReference.IsNullOrEmpty(contentLink))
                {
                    return _urlResolver.Service.GetUrl(contentLink);
                }
            }

            return _urlResolver.Service.GetUrl(ContentReference.StartPage);
        }

        /// <summary>
        /// Updates display name with current language.
        /// </summary>
        /// <param name="purchaseOrder">The purchase order.</param>
        public static void UpdateDisplayNameWithCurrentLanguage(IPurchaseOrder purchaseOrder)
        {
            if (purchaseOrder != null)
            {
                foreach (var lineItem in purchaseOrder.GetAllLineItems())
                {
                    lineItem.DisplayName = GetDisplayNameInCurrentLanguage(lineItem, 100);
                }
            }
        }

        /// <summary>
        /// Gets display name of line item in current language
        /// </summary>
        /// <param name="lineItem">The line item of the order.</param>
        /// <param name="maxSize">The number of character to get display name.</param>
        /// <returns>The display name with current language.</returns>
        private static string GetDisplayNameInCurrentLanguage(ILineItem lineItem, int maxSize)
        {
            // if the entry is null (product is deleted), return item display name
            var entryContent = _contentLoader.Service.Get<EntryContentBase>(_referenceConverter.Service.GetContentLink(lineItem.Code));
            var displayName = entryContent != null ? entryContent.DisplayName : lineItem.DisplayName;
            return StripPreviewText(displayName, maxSize <= 0 ? 100 : maxSize);
        }

        /// <summary>
        /// Strips a text to a given length without splitting the last word.
        /// </summary>
        /// <param name="source">The source string.</param>
        /// <param name="maxLength">Max length of the text.</param>
        /// <returns>A shortened version of the given string.</returns>
        public static string StripPreviewText(string source, int maxLength)
        {
            if (string.IsNullOrEmpty(source))
            {
                return string.Empty;
            }

            if (source.Length <= maxLength)
            {
                return source;
            }

            source = source.Substring(0, maxLength);
            // The maximum number of characters to cut from the end of the string.
            var maxCharCut = (source.Length > 15 ? 15 : source.Length - 1);
            var previousWord = source.LastIndexOfAny(new char[] { ' ', '.', ',', '!', '?' }, source.Length - 1, maxCharCut);
            if (previousWord >= 0)
            {
                source = source.Substring(0, previousWord);
            }
            return source + " ...";
        }

        public static bool ValidatePurchasePriceLimit(decimal cartTotal, OpenpayConfiguration configuration)
        {
            decimal.TryParse(configuration.MinPurchaseLimit, out var minLimit);
            decimal.TryParse(configuration.MaxPurchaseLimit, out var maxLimit);

            if ((minLimit == 0 && maxLimit == 0) || minLimit >= maxLimit || cartTotal < minLimit || cartTotal > maxLimit)
            {
                return false;
            }
            return true;
        }

        public static bool ValidateProhibitedProducts(ICart cart, string excludedProductConfigItemId)
        {
            if (cart == null || string.IsNullOrWhiteSpace(excludedProductConfigItemId))
            {
                return true;
            }

            // Check if current cart contain excluded product types
            foreach (var shipment in cart.Forms.SelectMany(x => x.Shipments))
            {
                var codes = shipment.LineItems.Select(x => x.Code);
                var entries = _contentLoader.Service.GetItems(
                    codes
                        .Select(x => _referenceConverter.Service.GetContentLink(x))
                        .Where(r => !ContentReference.IsNullOrEmpty(r)),
                    _preferredLang).OfType<EntryContentBase>().ToList();

                foreach (var lineItem in shipment.LineItems)
                {
                    var entry = entries.FirstOrDefault(x => x.Code == lineItem.Code);
                    if (entry != null && !ValidateContentItem(entry, excludedProductConfigItemId))
                    {
                        // stop if exist excluded product type
                        return false;
                    }
                }
            }

            return true;
        }


        /// <summary>
        /// Adds the note to purchase order.
        /// </summary>
        /// <param name="title">The note title.</param>
        /// <param name="detail">The note detail.</param>
        /// <param name="customerId">The customer Id.</param>
        /// <param name="purchaseOrder">The purchase order.</param>
        public static void AddNoteToPurchaseOrder(string title, string detail, Guid customerId, IPurchaseOrder purchaseOrder)
        {
            var orderNote = purchaseOrder.CreateOrderNote();
            orderNote.Type = OrderNoteTypes.System.ToString();
            orderNote.CustomerId = customerId != Guid.Empty ? customerId : PrincipalInfo.CurrentPrincipal.GetContactId();
            orderNote.Title = !string.IsNullOrEmpty(title) ? title : detail.Substring(0, Math.Min(detail.Length, 24)) + "...";
            orderNote.Detail = detail;
            orderNote.Created = DateTime.UtcNow;
            purchaseOrder.Notes.Add(orderNote);
        }


        #region private helpers

        public static bool ValidateContentItem(EntryContentBase entry, string excludedProductConfigItemId)
        {
            // Get configured product types are prohibited by Openpay
            var pageRef = new PageReference(excludedProductConfigItemId);
            var configPage = _contentLoader.Service.Get<OpenpayConfigurationPage>(pageRef);
            if (configPage?.ExcludedProductTypes == null || !configPage.ExcludedProductTypes.Items.Any())
                return true;

            var excludeIdList = configPage.ExcludedProductTypes.Items
                .Select(x => x.ContentLink)
                .Where(y => !ContentReference.IsNullOrEmpty(y)).ToList();
            _excludeEntryList = _contentLoader.Service.GetItems(excludeIdList, _preferredLang).OfType<EntryContentBase>().ToList();
            _excludeNodeList = _contentLoader.Service.GetItems(excludeIdList, _preferredLang).OfType<NodeContentBase>().ToList();

            var isValid = true;

            if (entry.ClassTypeId == "Variation")
            {
                if (_excludeEntryList.Select(x => x.ContentLink.ID).Contains(entry.ContentLink.ID) || !ValidateParentOfEntry(entry.ContentLink))
                {
                    isValid = false;
                }
            }
            else
            {
                if (_excludeEntryList.Select(x => x.ContentLink.ID).Contains(entry.ContentLink.ID) || !ValidateParentOfEntry(entry.ContentLink) || !ValidateChildrenOfEntry(entry.ContentLink))
                {
                    isValid = false;
                }
            }

            return isValid;
        }

        private static bool ValidateParentOfEntry(ContentReference entry)
        {
            var parentNodes = _relationRepository.Service.GetParents<NodeRelation>(entry);
            var parentEntries = _relationRepository.Service.GetParents<EntryRelation>(entry);

            foreach (var node in parentNodes)
            {
                if (_excludeNodeList.Select(x => x.ContentLink.ID).Contains(node.Parent.ID))
                {
                    return false;
                }
            }

            foreach (var parent in parentEntries)
            {
                if (_excludeEntryList.Select(x => x.ContentLink.ID).Contains(parent.Parent.ID))
                {
                    return false;
                }
            }

            return true;
        }

        private static bool ValidateChildrenOfEntry(ContentReference entry)
        {
            var childEntries = _relationRepository.Service.GetChildren<EntryRelation>(entry);

            foreach (var child in childEntries)
            {
                if (_excludeEntryList.Select(x => x.ContentLink.ID).Contains(child.Child.ID))
                {
                    return false;
                }

                if (!ValidateParentOfEntry(child.Child))
                {
                    return false;
                }
            }

            return true;
        }

        #endregion
    }
}
