using EPiServer.Shell.ObjectEditing;
using System.Collections.Generic;

namespace Openpay.EpiCommerce.AddOns.PaymentGateway.Models.Widget.SelectionFactory
{
    public class CartLogoSelectionFactory : ISelectionFactory
    {
        public IEnumerable<ISelectItem> GetSelections(ExtendedMetadata metadata)
        {
            return new ISelectItem[] {
                new SelectItem
                {
                    Text = "Grey Logo with Amber Background",
                    Value = "grey-on-amberbg"
                },
                new SelectItem
                {
                    Text = "Grey Logo",
                    Value = "grey"
                }
            };
        }
    }
}
