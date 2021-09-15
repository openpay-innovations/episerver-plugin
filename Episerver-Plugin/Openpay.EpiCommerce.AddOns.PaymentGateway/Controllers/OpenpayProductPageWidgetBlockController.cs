using EPiServer;
using EPiServer.Core;
using EPiServer.Web;
using EPiServer.Web.Mvc;
using System.Web.Mvc;
using Openpay.EpiCommerce.AddOns.PaymentGateway.BlockTypes;
using Openpay.EpiCommerce.AddOns.PaymentGateway.Constants;
using Openpay.EpiCommerce.AddOns.PaymentGateway.Helpers;

namespace Openpay.EpiCommerce.AddOns.PaymentGateway.Controllers
{
    public class OpenpayProductPageWidgetBlockController : BlockController<OpenpayProductPageWidgetBlock>
    {
        public override ActionResult Index(OpenpayProductPageWidgetBlock currentBlock)
        {
            var productPriceValue = ControllerContext.ParentActionViewContext.ViewData[CommonConstants.OpenpayProductPriceValue].ToString();
            decimal.TryParse(productPriceValue, out var productPrice);
            if (productPrice <= 0)
            {
                return null;
            }
            var model = WidgetViewModelFactory.GetProductPageWidgetViewModel(productPrice);
            return PartialView("OpenpayProductPageWidgetBlock", model);
        }
    }
}
