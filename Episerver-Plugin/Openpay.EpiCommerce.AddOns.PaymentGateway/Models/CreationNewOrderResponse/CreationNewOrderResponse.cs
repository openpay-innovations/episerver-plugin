using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Openpay.EpiCommerce.AddOns.PaymentGateway.Models.CreationNewOrderResponse
{
    public class CreationNewOrderResponse
    {
        public string OrderId { get; set; }
        public int BlackListMatch { get; set; }
        public NextAction NextAction { get; set; }
    }
}
