using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Openpay.EpiCommerce.AddOns.PaymentGateway.BlockTypes;

namespace Openpay.EpiCommerce.AddOns.PaymentGateway.Models.Widget
{
    public class CartWidgetViewModel : WidgetViewModelBase
    {
        public  decimal CartTotal { get; set; }
        public  string Logo { get; set; }
        public  string LearnMoreText { get; set; }
        public  bool ShowCartWidget { get; set; }
        public string InfoIcon { get; set; }
    }
}
