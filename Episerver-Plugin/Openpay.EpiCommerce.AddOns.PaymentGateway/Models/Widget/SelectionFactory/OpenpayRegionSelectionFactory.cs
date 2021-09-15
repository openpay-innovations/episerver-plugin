using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EPiServer.Shell.ObjectEditing;
using Openpay.EpiCommerce.AddOns.PaymentGateway.Enums;

namespace Openpay.EpiCommerce.AddOns.PaymentGateway.Models.Widget
{
    public class OpenpayRegionSelectionFactory : ISelectionFactory
    {
        public IEnumerable<ISelectItem> GetSelections(ExtendedMetadata metadata)
        {
            return new List<ISelectItem>
            {
                new SelectItem{ Text = OpenpayRegion.AU.ToString(), Value = OpenpayRegion.AU.ToString()},
                new SelectItem{ Text = OpenpayRegion.UK.ToString(), Value = OpenpayRegion.UK.ToString()}
            };
        }
    }
}
