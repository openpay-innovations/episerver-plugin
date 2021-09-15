using EPiServer;
using EPiServer.Core;
using EPiServer.Web;
using EPiServer.Web.Mvc;
using System.Web.Mvc;
using Openpay.EpiCommerce.AddOns.PaymentGateway.BlockTypes;
using Openpay.EpiCommerce.AddOns.PaymentGateway.Helpers;

namespace Openpay.EpiCommerce.AddOns.PaymentGateway.Controllers
{
    public class OpenpayCheckoutWidgetBlockController : BlockController<OpenpayCheckoutWidgetBlock>
    {
        public override ActionResult Index(OpenpayCheckoutWidgetBlock currentBlock)
        {
            var model = WidgetViewModelFactory.GetCheckoutWidgetViewModel();
            return PartialView("OpenpayCheckoutWidgetBlock", model);
        }
    }
}
