using EPiServer.Web.Mvc;
using System.Web.Mvc;
using Openpay.EpiCommerce.AddOns.PaymentGateway.BlockTypes;
using Openpay.EpiCommerce.AddOns.PaymentGateway.Helpers;

namespace Openpay.EpiCommerce.AddOns.PaymentGateway.Controllers
{
    public class OpenpayLearnMoreLinkWidgetBlockController : BlockController<OpenpayLearnMoreLinkWidgetBlock>
    {
        public override ActionResult Index(OpenpayLearnMoreLinkWidgetBlock currentBlock)
        {
            var model = WidgetViewModelFactory.GetLearnMoreLinkWidgetViewModel();
            return PartialView("LearnMoreLinkWidgetBlock", model);
        }
    }
}
