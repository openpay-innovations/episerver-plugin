using EPiServer.Core;
using EPiServer.DataAbstraction;
using EPiServer.DataAnnotations;
using System;
using System.ComponentModel.DataAnnotations;
using Openpay.EpiCommerce.AddOns.PaymentGateway.Constants;

namespace Openpay.EpiCommerce.AddOns.PaymentGateway.BlockTypes
{
    [ContentType(DisplayName = "Openpay Checkout Widget Block", GUID = "49e36c43-9659-4399-964c-cdc6613cbe33", GroupName = CommonConstants.OpenpayTabNames.OpenpayWidgetBlock)]
    [ImageUrl("~/styles/images/Openpay-apple-touch-icon.png")]
    public class OpenpayCheckoutWidgetBlock : BlockData
    {
    }
}