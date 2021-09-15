using EPiServer.Core;
using EPiServer.DataAnnotations;
using Openpay.EpiCommerce.AddOns.PaymentGateway.Constants;

namespace Openpay.EpiCommerce.AddOns.PaymentGateway.BlockTypes
{
    [ContentType(
        DisplayName = "Learn More Link Widget Block", 
        GUID = "d2b56e3e-bf3a-4c41-a983-bf7ea6dfb353", 
        Description = "The logo widget is a great way to show that Openpay is now available as a payment method on your website.", 
        GroupName = CommonConstants.OpenpayTabNames.OpenpayWidgetBlock)]
    [ImageUrl("~/styles/images/Openpay-apple-touch-icon.png")]
    public class OpenpayLearnMoreLinkWidgetBlock : BlockData
    {
    }
}