using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Openpay.EpiCommerce.AddOns.PaymentGateway.Models
{
    public class ErrorResponseResult
    {
        public string Title { get; set; }
        public string Status { get; set; }
        public string Detail { get; set; }
        public List<object> Errors { get; set; }
    }
}
