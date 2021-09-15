using EPiServer.Core;
using EPiServer.DataAbstraction;
using EPiServer.DataAnnotations;
using EPiServer.SpecializedProperties;
using System;
using System.ComponentModel.DataAnnotations;
using Openpay.EpiCommerce.AddOns.PaymentGateway.Constants;

namespace Openpay.EpiCommerce.AddOns.PaymentGateway.PageTypes
{
    [ContentType(DisplayName = "OpenpayLandingPage", GUID = "78da430c-731a-4cbe-afed-dc03af1f57d0", GroupName = CommonConstants.OpenpayTabNames.OpenpayPage)]
    public class OpenpayLandingPage : PageData
    {
    }
}