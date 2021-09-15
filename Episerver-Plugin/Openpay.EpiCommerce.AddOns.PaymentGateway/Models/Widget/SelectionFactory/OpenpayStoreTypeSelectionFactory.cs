using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EPiServer.Shell.ObjectEditing;
using Openpay.EpiCommerce.AddOns.PaymentGateway.Enums;

namespace Openpay.EpiCommerce.AddOns.PaymentGateway.Models.Widget
{
    public class OpenpayStoreTypeSelectionFactory :ISelectionFactory
    {
        public IEnumerable<ISelectItem> GetSelections(ExtendedMetadata metadata)
        {
            var types = new List<ISelectItem>
            {
                new SelectItem{ Text = OrderOrigin.Online.ToString(), Value = OrderOrigin.Online.ToString()},
                new SelectItem{ Text = OrderOrigin.Instore.ToString(), Value = OrderOrigin.Instore.ToString()}
            };
            metadata.InitialValue = types.First();
            return types;
        }
    }
}
