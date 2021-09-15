using EPiServer.Core;
using EPiServer.DataAbstraction;
using EPiServer.DataAnnotations;
using System;
using System.ComponentModel.DataAnnotations;
using EPiServer.Shell.ObjectEditing;
using EPiServer.Web;
using Openpay.EpiCommerce.AddOns.PaymentGateway.Constants;
using Openpay.EpiCommerce.AddOns.PaymentGateway.Models.Widget;

namespace Openpay.EpiCommerce.AddOns.PaymentGateway.BlockTypes
{
    [ContentType(DisplayName = "Openpay Info Belt Widget Block", GUID = "9e71fa36-0e4e-4b20-9acf-49b79333c26e", GroupName = CommonConstants.OpenpayTabNames.OpenpayWidgetBlock)]
    [ImageUrl("~/styles/images/Openpay-apple-touch-icon.png")]
    public class OpenpayInfoBeltWidgetBlock : BlockData
    {
    }
}