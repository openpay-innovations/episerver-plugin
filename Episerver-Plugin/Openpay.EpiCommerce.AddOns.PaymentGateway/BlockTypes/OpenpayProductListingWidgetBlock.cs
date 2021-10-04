using System.ComponentModel;
using EPiServer.Core;
using EPiServer.DataAbstraction;
using EPiServer.DataAnnotations;
using EPiServer.Shell.ObjectEditing;
using Openpay.EpiCommerce.AddOns.PaymentGateway.Models.Widget;
using System.ComponentModel.DataAnnotations;
using Openpay.EpiCommerce.AddOns.PaymentGateway.Constants;

namespace Openpay.EpiCommerce.AddOns.PaymentGateway.BlockTypes
{
    [ContentType(DisplayName = "Openpay Product Listing Widget Block", GUID = "f0902530-3bd9-413b-be91-e99ee35c77f6", GroupName = CommonConstants.OpenpayTabNames.OpenpayWidgetBlock)]
    [ImageUrl("~/styles/images/Openpay-apple-touch-icon.png")]
    public class OpenpayProductListingWidgetBlock : BlockData
    {
    }
}
