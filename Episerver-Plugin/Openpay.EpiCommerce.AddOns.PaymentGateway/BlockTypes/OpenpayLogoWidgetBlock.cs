using EPiServer.Core;
using EPiServer.DataAbstraction;
using EPiServer.DataAnnotations;
using System;
using System.ComponentModel.DataAnnotations;
using Openpay.EpiCommerce.AddOns.PaymentGateway.Constants;

namespace Openpay.EpiCommerce.AddOns.PaymentGateway.BlockTypes
{
    [ContentType(
        DisplayName = "Openpay Logo Widget Block", 
        GUID = "78c27b91-5469-4a70-bc9b-eaed6ba2c176", 
        Description = "The logo widget is a great way to show that Openpay is now available as a payment method on your website.",
        GroupName = CommonConstants.OpenpayTabNames.OpenpayWidgetBlock)]
    [ImageUrl("~/styles/images/Openpay-apple-touch-icon.png")]
    public class OpenpayLogoWidgetBlock : BlockData
    {
    }
}