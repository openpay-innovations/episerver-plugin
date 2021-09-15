using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Openpay.EpiCommerce.AddOns.PaymentGateway.Models.CreationNewOrderResponse
{
    public class FormPost
    {
        public string FormPostUrl { get; set; }
        public List<FormField> FormFields { get; set; }
    }
}
