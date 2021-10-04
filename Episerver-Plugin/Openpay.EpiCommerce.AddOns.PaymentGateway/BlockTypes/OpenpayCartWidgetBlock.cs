using EPiServer.Core;
using EPiServer.DataAbstraction;
using EPiServer.DataAnnotations;
using System;
using System.ComponentModel.DataAnnotations;
using EPiServer.Shell.ObjectEditing;
using Openpay.EpiCommerce.AddOns.PaymentGateway.Constants;
using Openpay.EpiCommerce.AddOns.PaymentGateway.Models.Widget;

namespace Openpay.EpiCommerce.AddOns.PaymentGateway.BlockTypes
{
    [ContentType(
        DisplayName = "Openpay Cart Widget Block",
        GUID = "34651e39-b353-4c20-a788-360687f8c883",
        Description = "The cart widget is a great way to show that Openpay is now available as a payment method on your website.",
        GroupName = CommonConstants.OpenpayTabNames.OpenpayWidgetBlock)]
    [ImageUrl("~/styles/images/Openpay-apple-touch-icon.png")]
    public class OpenpayCartWidgetBlock : BlockData
    {
    }
}