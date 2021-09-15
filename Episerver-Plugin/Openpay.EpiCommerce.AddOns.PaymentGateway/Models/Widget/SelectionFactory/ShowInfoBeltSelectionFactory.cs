using EPiServer.Shell.ObjectEditing;
using System.Collections.Generic;
using Openpay.EpiCommerce.AddOns.PaymentGateway.Enums;

namespace Openpay.EpiCommerce.AddOns.PaymentGateway.Models.Widget.SelectionFactory
{
    public class ShowInfoBeltSelectionFactory : ISelectionFactory
    {
        public IEnumerable<ISelectItem> GetSelections(ExtendedMetadata metadata)
        {
            return new ISelectItem[] {
                new SelectItem
                {
                    Text = "Home Page",
                    Value = ShowInfoBeltOption.HomePage.ToString()
                },
                new SelectItem
                {
                    Text = "All Page",
                    Value = ShowInfoBeltOption.AllPage.ToString()
                },
                new SelectItem
                {
                    Text = "No",
                    Value = ShowInfoBeltOption.No.ToString()
                }
            };
        }
    }
}
