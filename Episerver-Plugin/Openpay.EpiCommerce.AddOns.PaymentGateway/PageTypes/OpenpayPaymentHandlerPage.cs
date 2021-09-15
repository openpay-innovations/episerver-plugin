using EPiServer.Core;
using EPiServer.DataAnnotations;
using System.ComponentModel.DataAnnotations;
using EPiServer.DataAbstraction;
using Openpay.EpiCommerce.AddOns.PaymentGateway.Constants;

namespace Openpay.EpiCommerce.AddOns.PaymentGateway.PageTypes
{
    [ContentType(GUID = "9836b8ac-1097-47dc-9e28-5f5223237966",
        DisplayName = "Openpay Payment Page",
        Description = "Openpay Payment handler process page.",
        GroupName = CommonConstants.OpenpayTabNames.OpenpayPage,
        Order = 100)]
    [ImageUrl("~/styles/images/Openpay-apple-touch-icon.png")]
    public class OpenpayPaymentHandlerPage : PageData
    {
        [Display(
            Name = "Order Complete Page",
            Description = "Page which should be redirected to when order is completed",
            GroupName = SystemTabNames.Content,
            Order = 1)]
        [Required]
        [AllowedTypes(typeof(PageData))]
        public virtual ContentReference OrderCompletePage { get; set; }

        [Display(
            Name = "Order Cancel Page",
            Description = "Page which should be redirected to when order is cancelled",
            GroupName = SystemTabNames.Content,
            Order = 1)]
        [Required]
        [AllowedTypes(typeof(PageData))]
        public virtual ContentReference OrderCancelPage { get; set; }
    }
}