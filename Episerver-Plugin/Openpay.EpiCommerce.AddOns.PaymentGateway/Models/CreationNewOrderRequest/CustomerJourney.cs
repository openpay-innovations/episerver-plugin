using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Openpay.EpiCommerce.AddOns.PaymentGateway.Models.CreationNewOrderRequest
{
    public class CustomerJourney
    {
        public string Origin { get; set; }
        public OnlineOrigin Online { get; set; }
    }
}
