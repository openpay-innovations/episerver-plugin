using EPiServer.Core;
using EPiServer.DataAbstraction;
using EPiServer.DataAnnotations;
using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using Openpay.EpiCommerce.AddOns.PaymentGateway.Constants;

namespace Openpay.EpiCommerce.AddOns.PaymentGateway.BlockTypes
{
    [ContentType(DisplayName = "Openpay Product Page Widget Block", GUID = "7b325cc4-91c2-4340-bf5d-7646a77314fc", GroupName = CommonConstants.OpenpayTabNames.OpenpayWidgetBlock)]
    [ImageUrl("~/styles/images/Openpay-apple-touch-icon.png")]
    public class OpenpayProductPageWidgetBlock : BlockData
    {
    }
}