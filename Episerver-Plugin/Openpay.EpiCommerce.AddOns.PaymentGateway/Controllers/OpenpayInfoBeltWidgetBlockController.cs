using EPiServer;
using EPiServer.Core;
using EPiServer.Web;
using EPiServer.Web.Mvc;
using System.Web.Mvc;
using Openpay.EpiCommerce.AddOns.PaymentGateway.BlockTypes;
using Openpay.EpiCommerce.AddOns.PaymentGateway.Helpers;

namespace Openpay.EpiCommerce.AddOns.PaymentGateway.Controllers
{
    public class OpenpayInfoBeltWidgetBlockController : BlockController<OpenpayInfoBeltWidgetBlock>
    {
        public override ActionResult Index(OpenpayInfoBeltWidgetBlock currentBlock)
        {
            var model = WidgetViewModelFactory.GetInfoBeltWidgetViewModel();
            return PartialView("OpenpayInfoBeltWidgetBlock", model);
        }
    }
}
