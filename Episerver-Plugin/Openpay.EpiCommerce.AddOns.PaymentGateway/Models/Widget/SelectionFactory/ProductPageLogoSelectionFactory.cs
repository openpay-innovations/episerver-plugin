using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EPiServer.Shell.ObjectEditing;

namespace Openpay.EpiCommerce.AddOns.PaymentGateway.Models.Widget.SelectionFactory
{
    public class ProductPageLogoSelectionFactory : ISelectionFactory
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
