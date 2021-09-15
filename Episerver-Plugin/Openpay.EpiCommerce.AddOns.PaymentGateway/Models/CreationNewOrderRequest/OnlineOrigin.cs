using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Openpay.EpiCommerce.AddOns.PaymentGateway.Models.CreationNewOrderRequest
{
    public class OnlineOrigin
    {
        public string CallbackUrl { get; set; }
        public string CancelUrl { get; set; }
        public string FailUrl { get; set; }
        public string PlanCreationType { get; set; }
        public CustomerDetails CustomerDetails { get; set; }
        public string DeliveryMethod { get; set; }
    }
}
