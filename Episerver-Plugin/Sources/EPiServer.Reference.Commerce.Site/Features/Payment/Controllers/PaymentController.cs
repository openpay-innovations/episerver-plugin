using EPiServer.Commerce.Order;
using EPiServer.Reference.Commerce.Site.Features.Payment.ViewModelFactories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using EPiServer.Commerce.Catalog.ContentTypes;
using EPiServer.Commerce.Catalog.Linking;
using EPiServer.Core;
using EPiServer.Globalization;
using EPiServer.Logging;
using EPiServer.Reference.Commerce.Site.Features.Cart.Services;
using EPiServer.Reference.Commerce.Site.Features.Payment.Services;
using EPiServer.Reference.Commerce.Site.Features.Payment.ViewModels;
using EPiServer.Reference.Commerce.Site.Features.Product.Models;
using EPiServer.ServiceLocation;
using Mediachase.Commerce.Catalog;
using Mediachase.Commerce.Core;
using Mediachase.Commerce.Orders.Dto;
using Openpay.EpiCommerce.AddOns.PaymentGateway.Constants;
using Openpay.EpiCommerce.AddOns.PaymentGateway;
using Openpay.EpiCommerce.AddOns.PaymentGateway.Helpers;
using Openpay.EpiCommerce.AddOns.PaymentGateway.PageTypes;

namespace EPiServer.Reference.Commerce.Site.Features.Payment.Controllers
{
    public class PaymentController : Controller
    {
        private readonly PaymentMethodViewModelFactory _paymentMethodViewModelFactory;
        readonly IOrderGroupCalculator _orderGroupCalculator;
        private readonly ICartService _cartService;
        private readonly IPaymentManagerFacade _paymentManager;
        private readonly IContentLoader _contentLoader;
        private readonly ReferenceConverter _referenceConverter;
        private readonly LanguageResolver _languageResolver;
        private readonly IRelationRepository _relationRepository;
        private ICart Cart => _cartService.LoadCart(_cartService.DefaultCartName);
        private List<NodeContentBase> _excludeNodeList;
        private List<EntryContentBase> _excludeEntryList;
        private bool _hasExcludedItem;

        public PaymentController(
            PaymentMethodViewModelFactory paymentMethodViewModelFactory,
            ICartService cartService,
            IOrderGroupCalculator orderGroupCalculator,
            IPaymentManagerFacade paymentManager,
            IContentLoader contentLoader,
            ReferenceConverter referenceConverter,
            LanguageResolver languageResolver,
            IRelationRepository relationRepository
            )
        {
            _paymentMethodViewModelFactory = paymentMethodViewModelFactory;
            _orderGroupCalculator = orderGroupCalculator;
            _cartService = cartService;
            _paymentManager = paymentManager;
            _contentLoader = contentLoader;
            _referenceConverter = referenceConverter;
            _languageResolver = languageResolver;
            _relationRepository = relationRepository;
        }

        [HttpPost]
        public PartialViewResult SetPaymentMethod(Guid paymentMethodId)
        {
            var viewModel = _paymentMethodViewModelFactory.CreatePaymentMethodSelectionViewModel(paymentMethodId);

            return PartialView("_PaymentMethodSelection", viewModel);
        }

        [AcceptVerbs(HttpVerbs.Get | HttpVerbs.Post)]
        public PartialViewResult PaymentMethodSelection(IPaymentMethod payment)
        {
            var viewModel = _paymentMethodViewModelFactory.CreatePaymentMethodSelectionViewModel(payment);

            // get parameters from Created Openpay payment method
            var parameters =
                _paymentManager.GetPaymentMethodBySystemName(OpenpayConfigurationConstants.OpenpaySystemName,
                    SiteContext.Current.LanguageName)?.PaymentMethodParameter;

            if (RestrictOpenpayPurchaseLimit(ref viewModel, parameters))
            {
                return PartialView("_PaymentMethodSelection", viewModel);
            }

            viewModel = RestrictOpenpayProductType(viewModel, parameters);

            return PartialView("_PaymentMethodSelection", viewModel);
        }

        #region Restrict Openpay Purchase Limits

        private bool RestrictOpenpayPurchaseLimit(ref PaymentMethodSelectionViewModel viewModel, PaymentMethodDto.PaymentMethodParameterDataTable parameters)
        {
            viewModel.PaymentMethods = viewModel.PaymentMethods.Where(x => x.PaymentMethod != null).ToList();
            var openpayPaymentMethod = viewModel.PaymentMethods.FirstOrDefault(x =>
                x.PaymentMethod.SystemKeyword == OpenpayConfigurationConstants.OpenpaySystemName);

            if (openpayPaymentMethod != null)
            {
                var totals = _orderGroupCalculator.GetOrderGroupTotals(Cart).Total;
                var minPurchaseLimit = parameters?.FirstOrDefault(x => x.Parameter == OpenpayConfigurationConstants.MinPurchaseLimitParam);
                var maxPurchaseLimit = parameters?.FirstOrDefault(x => x.Parameter == OpenpayConfigurationConstants.MaxPurchaseLimitParam);
                if (minPurchaseLimit != null)
                {
                    decimal minPrice;
                    decimal.TryParse(minPurchaseLimit.Value, out minPrice);
                    if (minPrice > 0 && totals < minPrice)
                    {
                        RemoveOpenpayFromPaymentList(ref viewModel);
                        return true;
                    }
                }

                if (maxPurchaseLimit != null)
                {
                    decimal maxPrice;
                    decimal.TryParse(maxPurchaseLimit.Value, out maxPrice);
                    if (maxPrice > 0 && totals > maxPrice)
                    {
                        RemoveOpenpayFromPaymentList(ref viewModel);
                        return true;
                    }
                }
            }
            return false;
        }

        #endregion

        #region Restrict Openpay Excluded Product Types

        private PaymentMethodSelectionViewModel RestrictOpenpayProductType(PaymentMethodSelectionViewModel viewModel, PaymentMethodDto.PaymentMethodParameterDataTable parameters)
        {
            var configItemId = parameters?.FirstOrDefault(x => x.Parameter == OpenpayConfigurationConstants.ExcludedProductConfigItem);
            if (configItemId == null || string.IsNullOrWhiteSpace(configItemId.Value))
            {
                return viewModel;
            }

            var pageRef = new PageReference(configItemId.Value);
                
                // Get configured product types needed to be excluded
                var configPage = _contentLoader.Get<OpenpayConfigurationPage>(pageRef);
                if (configPage?.ExcludedProductTypes == null || !configPage.ExcludedProductTypes.Items.Any())
                    return viewModel;

                var excludeIdList = configPage.ExcludedProductTypes.Items
                    .Select(x => x.ContentLink)
                    .Where(y => !ContentReference.IsNullOrEmpty(y));
                _excludeEntryList = _contentLoader.GetItems(excludeIdList, _languageResolver.GetPreferredCulture()).OfType<EntryContentBase>().ToList();
                _excludeNodeList = _contentLoader.GetItems(excludeIdList, _languageResolver.GetPreferredCulture()).OfType<NodeContentBase>().ToList();

                // Check if current cart contain excluded product types
                foreach (var shipment in Cart.Forms.SelectMany(x => x.Shipments))
                {
                    var codes = shipment.LineItems.Select(x => x.Code);
                    var entries = _contentLoader.GetItems(
                        codes
                            .Select(x => _referenceConverter.GetContentLink(x))
                            .Where(r => !ContentReference.IsNullOrEmpty(r)),
                            _languageResolver.GetPreferredCulture()).OfType<EntryContentBase>();

                    foreach (var lineItem in shipment.LineItems)
                    {
                        var entry = entries.FirstOrDefault(x => x.Code == lineItem.Code);
                        if (entry != null)
                        {
                            ValidateCartItem(entry);
                        }

                        // stop if exist excluded product type
                        if (_hasExcludedItem)
                            break;
                    }

                    // stop if exist excluded product type
                    if (_hasExcludedItem)
                        break;
                }

                if (_hasExcludedItem)
                {
                    RemoveOpenpayFromPaymentList(ref viewModel);
                }

            return viewModel;
        }

        private void ValidateCartItem(EntryContentBase entry)
        {
            switch (entry.ClassTypeId)
            {
                case "Variation":
                    if (_excludeEntryList.Select(x => x.ContentLink.ID).Contains(entry.ContentLink.ID))
                    {
                        _hasExcludedItem = true;
                        break;
                    }
                    ValidateParentOfEntry(entry.ContentLink);

                    break;

                case "Package":
                    if (_excludeEntryList.Select(x => x.ContentLink.ID).Contains(entry.ContentLink.ID))
                    {
                        _hasExcludedItem = true;
                        break;
                    }
                    ValidateParentOfEntry(entry.ContentLink);
                    ValidateChildrenOfEntry(entry.ContentLink);

                    break;
            }
        }

        private void ValidateParentOfEntry(ContentReference entry)
        {
            var parentNodes = _relationRepository.GetParents<NodeRelation>(entry);
            var parentEntries = _relationRepository.GetParents<EntryRelation>(entry);

            foreach (var node in parentNodes)
            {
                if (_excludeNodeList.Select(x => x.ContentLink.ID).Contains(node.Parent.ID))
                {
                    _hasExcludedItem = true;
                    return;
                }
            }

            foreach (var parent in parentEntries)
            {
                if (_excludeEntryList.Select(x => x.ContentLink.ID).Contains(parent.Parent.ID))
                {
                    _hasExcludedItem = true;
                    return;
                }
            }
        }

        private void ValidateChildrenOfEntry(ContentReference entry)
        {
            var childEntries = _relationRepository.GetChildren<EntryRelation>(entry);

            foreach (var child in childEntries)
            {
                if (_excludeEntryList.Select(x => x.ContentLink.ID).Contains(child.Child.ID))
                {
                    _hasExcludedItem = true;
                    return;
                }

                ValidateParentOfEntry(child.Child);
            }
        }

        #endregion

        private void RemoveOpenpayFromPaymentList(ref PaymentMethodSelectionViewModel viewModel)
        {
            viewModel.PaymentMethods = viewModel.PaymentMethods.Where(x =>
                x.PaymentMethod.SystemKeyword != OpenpayConfigurationConstants.OpenpaySystemName);
            if (viewModel.SelectedPaymentMethod.PaymentMethod.SystemKeyword == OpenpayConfigurationConstants.OpenpaySystemName)
            {
                viewModel.SelectedPaymentMethod = viewModel.PaymentMethods.First();
            }
        }

    }
}