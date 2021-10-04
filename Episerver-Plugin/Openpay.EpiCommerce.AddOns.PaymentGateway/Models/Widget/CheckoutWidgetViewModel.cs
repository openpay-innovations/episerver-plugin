using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EPiServer.Core;

namespace Openpay.EpiCommerce.AddOns.PaymentGateway.Models.Widget
{
    public class CheckoutWidgetViewModel : WidgetViewModelBase
    {
        public bool ShowLogoWidget { get; set; }
        public bool ShowLearnMoreWidget { get; set; }
        public bool ShowCheckoutWidget { get; set; }
        public XhtmlString InstalmentText { get; set; }
        public string Logo { get; set; }
        public string LearnMore { get; set; }
        public string InfoIcon { get; set; }
    }
}
