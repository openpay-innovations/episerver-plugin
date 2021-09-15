using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EPiServer.Commerce.Order;
using EPiServer.ServiceLocation;
using Mediachase.Commerce.Orders;
using Mediachase.Commerce.Orders.Dto;
using Openpay.EpiCommerce.AddOns.PaymentGateway.Helpers;

namespace Openpay.EpiCommerce.AddOns.PaymentGateway
{
    [ServiceConfiguration(typeof(IPaymentMethod))]
    public class OpenpayPaymentMethod : IPaymentMethod
    {
        private readonly IOrderGroupFactory _orderGroupFactory;
        private readonly PaymentMethodDto _paymentMethodDto;
        private readonly PaymentMethodDto.PaymentMethodRow _paymentMethod;

        public Guid PaymentMethodId { get; }
        public string SystemKeyword { get; }
        public string Name { get; }
        public string Description { get; }

        public OpenpayPaymentMethod() : this(ServiceLocator.Current.GetInstance<IOrderGroupFactory>())
        {
        }

        public OpenpayPaymentMethod(IOrderGroupFactory orderGroupFactory)
        {
            _orderGroupFactory = orderGroupFactory;
            _paymentMethodDto = OpenpayConfiguration.GetOpenpayPaymentMethod();
            _paymentMethod = _paymentMethodDto?.PaymentMethod?.FirstOrDefault();

            if (_paymentMethod == null)
            {
                return;
            }

            PaymentMethodId = _paymentMethod.PaymentMethodId;
            SystemKeyword = _paymentMethod.SystemKeyword;
            Name = _paymentMethod.Name;
            Description = _paymentMethod.Description;
        }

        public IPayment CreatePayment(decimal amount, IOrderGroup orderGroup)
        {
            var payment = orderGroup.CreatePayment(_orderGroupFactory, typeof(OpenpayPayment));
            
            payment.PaymentMethodId = _paymentMethod.PaymentMethodId;
            payment.PaymentMethodName = _paymentMethod.Name;
            payment.Amount = amount;
            payment.Status = PaymentStatus.Pending.ToString();
            payment.TransactionType = TransactionType.Authorization.ToString();

            return payment;
        }

        public bool ValidateData()
        {
            return true;
        }
    }
}
