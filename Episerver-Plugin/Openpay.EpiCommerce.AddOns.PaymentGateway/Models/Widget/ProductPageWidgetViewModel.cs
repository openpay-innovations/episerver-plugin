using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Openpay.EpiCommerce.AddOns.PaymentGateway.Models.Widget
{
    public class ProductPageWidgetViewModel : WidgetViewModelBase
    {
        public decimal ProductPrice { get; set; }
        public string ProductPageLogo { get; set; }
        public string ProductPageLogoPosition { get; set; }
        public string ProductPageLearnMoreText { get; set; }
        public bool ShowProductPageWidget { get; set; }
        public string InfoIcon { get; set; }
    }
}
