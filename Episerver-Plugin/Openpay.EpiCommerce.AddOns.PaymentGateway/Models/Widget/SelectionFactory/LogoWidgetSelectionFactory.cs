using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EPiServer.Shell.ObjectEditing;

namespace Openpay.EpiCommerce.AddOns.PaymentGateway.Models.Widget.SelectionFactory
{
    class LogoWidgetSelectionFactory : ISelectionFactory
    {
        public IEnumerable<ISelectItem> GetSelections(ExtendedMetadata metadata)
        {
            return new ISelectItem[] {
                new SelectItem
                {
                    Text = "Grey Logo with Amber Background",
                    Value = "grey-filled"
                },
                new SelectItem
                {
                    Text = "Grey Logo",
                    Value = "grey"
                },
                new SelectItem
                {
                    Text = "Amber Logo",
                    Value = "amber"
                }
            };
        }
    }
}
