namespace Openpay.EpiCommerce.AddOns.PaymentGateway.Models
{
    public class Order 
    {
        public string OrderId { get; set; }
        public int? BlackListMatch { get; set; }
        public string OrderStatus { get; set; }
        public string PlanStatus { get; set; }
        public int PurchasePrice { get; set; }
        public int RetailerAmount { get; set; }
        public object NextAction { get; set; }
    }
}
