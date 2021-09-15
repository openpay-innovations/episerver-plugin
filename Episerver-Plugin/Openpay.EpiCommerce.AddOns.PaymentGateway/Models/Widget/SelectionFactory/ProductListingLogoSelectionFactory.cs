using EPiServer.Shell.ObjectEditing;
using System.Collections.Generic;
using Openpay.EpiCommerce.AddOns.PaymentGateway.Enums;

namespace Openpay.EpiCommerce.AddOns.PaymentGateway.Models.Widget.SelectionFactory
{
    public class ProductListingLogoSelectionFactory : ISelectionFactory
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
                    Text = "White",
                    Value = "white"
                }
            };
        }
    }
}
