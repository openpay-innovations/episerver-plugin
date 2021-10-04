using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Openpay.EpiCommerce.AddOns.PaymentGateway.Models
{
    public class CapturePaymentResponse
    {
        public string OrderId { get; set; }
        public string PurchasePrice { get; set; }
    }
}
