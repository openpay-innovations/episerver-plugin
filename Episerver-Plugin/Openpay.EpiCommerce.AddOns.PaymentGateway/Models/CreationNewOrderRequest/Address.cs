using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Openpay.EpiCommerce.AddOns.PaymentGateway.Models.CreationNewOrderRequest
{
    public class Address
    {
        public string Line1 { get; set; }
        public string Line2 { get; set; }
        public string Suburb { get; set; }
        public string State { get; set; }
        public string PostCode { get; set; }
    }
}
