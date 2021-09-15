using EPiServer.Web.Mvc;
using System.Web.Mvc;
using EPiServer.Commerce.Order;
using EPiServer.Security;
using Mediachase.Commerce.Orders;
using Mediachase.Commerce.Security;
using Openpay.EpiCommerce.AddOns.PaymentGateway.BlockTypes;
using Openpay.EpiCommerce.AddOns.PaymentGateway.Helpers;
using Openpay.EpiCommerce.AddOns.PaymentGateway.Models.Widget;

namespace Openpay.EpiCommerce.AddOns.PaymentGateway.Controllers
{
    public class OpenpayCartWidgetBlockController : BlockController<OpenpayCartWidgetBlock>
    {
        private readonly IOrderRepository _orderRepository;
        public OpenpayCartWidgetBlockController(IOrderRepository orderRepository)
        {
            _orderRepository = orderRepository;
        }

        public override ActionResult Index(OpenpayCartWidgetBlock currentBlock)
        {
            var model = WidgetViewModelFactory.GetCarWidgetViewModel();
            return PartialView("OpenpayCartWidgetBlock", model);
        }
    }
}
