using Openpay.EpiCommerce.AddOns.PaymentGateway.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Openpay.EpiCommerce.AddOns.PaymentGateway.Models.CreationNewOrderResponse
{
    public class NextAction
    {
        public NextActionType Type { get; set; }
        public FormPost FormPost { get; set; }
    }
}
