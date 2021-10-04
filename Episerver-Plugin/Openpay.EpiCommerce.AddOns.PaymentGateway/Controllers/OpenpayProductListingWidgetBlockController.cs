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
    public class OpenpayProductListingWidgetBlockController : BlockController<OpenpayProductListingWidgetBlock>
    {
        public override ActionResult Index(OpenpayProductListingWidgetBlock currentBlock)
        {
            var productPriceValue = ControllerContext.ParentActionViewContext.ViewData[CommonConstants.OpenpayProductPriceValue].ToString();
            var productCode = ControllerContext.ParentActionViewContext.ViewData[CommonConstants.OpenpayProductPriceValue].ToString();
            decimal.TryParse(productPriceValue, out var productPrice);
            if (productPrice <= 0)
            {
                return null;
            }

            var model = WidgetViewModelFactory.GetProductListingWidgetViewModel(productPrice, productCode);
            return PartialView("OpenpayProductListingWidgetBlock", model);
        }
    }
}
