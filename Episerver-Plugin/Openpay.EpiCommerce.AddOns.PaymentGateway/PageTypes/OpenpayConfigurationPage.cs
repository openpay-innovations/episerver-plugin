using EPiServer.Core;
using EPiServer.DataAbstraction;
using EPiServer.DataAnnotations;
using EPiServer.SpecializedProperties;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using EPiServer.Commerce.Catalog.ContentTypes;
using Openpay.EpiCommerce.AddOns.PaymentGateway.Constants;

namespace Openpay.EpiCommerce.AddOns.PaymentGateway
{
    [ContentType(
        DisplayName = "Openpay Configuration",
        GUID = "02857125-a7e3-4bcb-a7e5-d5ce08dff206", 
        Description = "Configure which Product types should be excluded",
        GroupName = CommonConstants.OpenpayTabNames.OpenpayPage)]
    public class OpenpayConfigurationPage : PageData
    {
        [Display(
            Name = "Product Catalog to be exclude",
            Description = "Define which Product Catalogs should be excluded from Openpay Payment option",
            GroupName = SystemTabNames.Content,
            Order = 1)]
        [AllowedTypes(
            typeof(CatalogContent),
            typeof(NodeContent),
            typeof(PackageContent),
            typeof(ProductContent),
            typeof(VariationContent),
            typeof(BundleContent))]
        public virtual ContentArea ExcludedProductTypes { get; set; }
    }
}