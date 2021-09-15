using EPiServer;
using EPiServer.Core;
using EPiServer.Web;
using EPiServer.Web.Mvc;
using System.Web.Mvc;
using Openpay.EpiCommerce.AddOns.PaymentGateway.BlockTypes;
using Openpay.EpiCommerce.AddOns.PaymentGateway.Helpers;

namespace Openpay.EpiCommerce.AddOns.PaymentGateway.Controllers
{
    public class OpenpayLogoWidgetBlockController : BlockController<OpenpayLogoWidgetBlock>
    {
        public override ActionResult Index(OpenpayLogoWidgetBlock currentBlock)
        {
            var model = WidgetViewModelFactory.GetLogoWidgetViewModel();
            return PartialView("OpenpayLogoWidgetBlock", model);
        }
    }
}
