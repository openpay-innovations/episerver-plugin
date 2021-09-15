using EPiServer.Shell.ObjectEditing;
using System.Collections.Generic;

namespace Openpay.EpiCommerce.AddOns.PaymentGateway.Models.Widget.SelectionFactory
{
    public class InfoIconSelectionFactory : ISelectionFactory
    {
        public IEnumerable<ISelectItem> GetSelections(ExtendedMetadata metadata)
        {
            return new ISelectItem[] {
                new SelectItem
                {
                    Text = "Grey",
                    Value = "grey"
                },
                new SelectItem
                {
                    Text = "Amber",
                    Value = "amber"
                },
                new SelectItem
                {
                    Text = "White",
                    Value = "white"
                }
            };
        }
    }
}
