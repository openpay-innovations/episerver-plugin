using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Openpay.EpiCommerce.AddOns.PaymentGateway.Models.CreationNewOrderRequest
{
    public class CartItem
    {
        public string ItemName { get; set; }
        public string ItemGroup { get; set; }
        public string ItemCode { get; set; }
        public string ItemGroupCode { get; set; }
        public int ItemRetailUnitPrice { get; set; }
        public int ItemQty { get; set; }
        public int ItemRetailCharge { get; set; }
    }
}
