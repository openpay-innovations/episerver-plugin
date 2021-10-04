using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Openpay.EpiCommerce.AddOns.PaymentGateway.Models.CreationNewOrderRequest
{
    public class CustomerDetails
    {
        public string FirstName { get; set; }
        public string OtherNames { get; set; }
        public string FamilyName { get; set; }
        public string Email { get; set; }
        public string DateOfBirth { get; set; }
        public string Gender { get; set; }
        public string PhoneNumber { get; set; }
        public Address ResidentialAddress { get; set; }
        public Address DeliveryAddress { get; set; }
    }
}
