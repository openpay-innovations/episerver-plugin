using Openpay.EpiCommerce.AddOns.PaymentGateway.BlockTypes;

namespace Openpay.EpiCommerce.AddOns.PaymentGateway.Models.Widget
{
    public class InfoBeltWidgetViewModel : WidgetViewModelBase
    {
        public bool ShowWidget { get; set; }
        public string Logo { get; set; }
        public bool StickyInfoBeltWidget { get; set; }
        public string StickyScript { get; set; }
    }
}
