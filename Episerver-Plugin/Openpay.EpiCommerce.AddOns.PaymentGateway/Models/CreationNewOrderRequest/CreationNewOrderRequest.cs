using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Openpay.EpiCommerce.AddOns.PaymentGateway.Models.CreationNewOrderRequest
{
    public class CreationNewOrderRequest
    {
        public CustomerJourney CustomerJourney { get; set; }
        public string GoodsDescription { get; set; }
        public int PurchasePrice { get; set; }
        public string RetailerOrderNo { get; set; }
        public string Source { get; set; }
        public List<CartItem> Cart { get; set; }
    }
}
