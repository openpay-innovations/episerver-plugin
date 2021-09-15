using System.Collections.Generic;
using System.Linq;
using EPiServer.Commerce.Order;
using EPiServer.ServiceLocation;
using Mediachase.Commerce;
using Mediachase.Commerce.Customers;

namespace Openpay.EpiCommerce.AddOns.PaymentGateway.Helpers
{
    public class CartService
    {
        private readonly IOrderRepository _orderRepository = ServiceLocator.Current.GetInstance<IOrderRepository>();
        private readonly ICurrentMarket _currentMarket = ServiceLocator.Current.GetInstance<ICurrentMarket>();
        private readonly OrderValidationService _orderValidationService = ServiceLocator.Current.GetInstance<OrderValidationService>();
        public string DefaultCartName => "Default";

        public ICart LoadCart(string name)
        {
            var cart = _orderRepository.LoadCart<ICart>(CustomerContext.Current.CurrentContactId, name, _currentMarket);
            if (cart != null)
            {
                var validationIssues = ValidateCart(cart);
                // After validate, if there is any change in cart, saving cart.
                if (validationIssues.Any())
                {
                    _orderRepository.Save(cart);
                }
            }
            return cart;
        }

        public IDictionary<ILineItem, IList<ValidationIssue>> ValidateCart(ICart cart)
        {
            return _orderValidationService.ValidateOrder(cart);
        }
    }
}
