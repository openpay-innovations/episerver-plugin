using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using Mediachase.Commerce.Orders;
using Mediachase.MetaDataPlus.Configurator;

namespace Openpay.EpiCommerce.AddOns.PaymentGateway
{
    public class OpenpayPayment : Payment
    {
        private static MetaClass _metaClass;
        public static MetaClass OpenpayPaymentMetaClass => _metaClass ?? (_metaClass = MetaClass.Load(OrderContext.MetaDataContext, "OpenpayPayment"));

        public OpenpayPayment() : base(OpenpayPaymentMetaClass)
        {
            PaymentType = PaymentType.Other;
            ImplementationClass = GetType().AssemblyQualifiedName; // need to have assembly name in order to retrieve the correct type in ClassInfo
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="OpenpayPayment"/> class.
        /// </summary>
        /// <param name="info">The info.</param>
        /// <param name="context">The context.</param>
        protected OpenpayPayment(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
            PaymentType = PaymentType.Other;
            ImplementationClass = GetType().AssemblyQualifiedName; // need to have assembly name in order to retrieve the correct type in ClassInfo
        }
    }
}
