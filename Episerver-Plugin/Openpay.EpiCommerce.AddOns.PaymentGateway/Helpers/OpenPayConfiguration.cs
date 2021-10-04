using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.UI.WebControls;
using Mediachase.Commerce.Core;
using Mediachase.Commerce.Orders.Dto;
using Mediachase.Commerce.Orders.Managers;
using Openpay.EpiCommerce.AddOns.PaymentGateway.Constants;
using Openpay.EpiCommerce.AddOns.PaymentGateway.Enums;

namespace Openpay.EpiCommerce.AddOns.PaymentGateway.Helpers
{
    public class OpenpayConfiguration
    {
        private PaymentMethodDto _paymentMethodDto;
        private static IDictionary<string, string> _settings;

        public Guid PaymentMethodId { get; protected set; }

        public string Region { get; protected set; }

        public OpenpayRegion SelectedRegion
        {
            get
            {
                Enum.TryParse(Region, out OpenpayRegion selectedRegion);
                return selectedRegion;
            }
        }

        public string Description { get; protected set; }
        public string Environment { get; protected set; }
        public OpenpayEnvironment SelectedEnvironment
        {
            get
            {
                Enum.TryParse(Environment, out OpenpayEnvironment selectedEnvironment);
                return selectedEnvironment;
            }
        }
        public string Username { get; protected set; }
        public string Password { get; protected set; }
        public string ExcludedProductConfigItem { get; protected set; }
        public string MinPurchaseLimit { get; protected set; }
        public string MaxPurchaseLimit { get; protected set; }
        public string CallbackItemId { get; protected set; }

        public OpenpayConfiguration() : this(null)
        {
        }

        public OpenpayConfiguration(IDictionary<string, string> settings)
        {
            Initialize(settings);
        }


        protected virtual void Initialize(IDictionary<string, string> settings)
        {
            _paymentMethodDto = GetOpenpayPaymentMethod();
            PaymentMethodId = GetPaymentMethodId();

            _settings = settings ?? GetSettings();
            GetParametersValues();
        }

        public static PaymentMethodDto GetOpenpayPaymentMethod()
        {
            return PaymentManager.GetPaymentMethodBySystemName(OpenpayConfigurationConstants.OpenpaySystemName, SiteContext.Current.LanguageName);
        }

        public string GetParameterValue(string parameterName)
        {
            string parameterValue;
            return _settings.TryGetValue(parameterName, out parameterValue) ? parameterValue : string.Empty;
        }

        #region Private Method

        private Guid GetPaymentMethodId()
        {
            var paymentMethodRow = _paymentMethodDto.PaymentMethod.Rows[0] as PaymentMethodDto.PaymentMethodRow;
            return paymentMethodRow != null ? paymentMethodRow.PaymentMethodId : Guid.Empty;
        }

        private IDictionary<string, string> GetSettings()
        {
            return _paymentMethodDto.PaymentMethod
                                    .FirstOrDefault()
                                   ?.GetPaymentMethodParameterRows()
                                   ?.ToDictionary(row => row.Parameter, row => row.Value);
        }

        private void GetParametersValues()
        {
            if (_settings != null)
            {
                Region = GetParameterValue(OpenpayConfigurationConstants.RegionParam);
                Description = GetParameterValue(OpenpayConfigurationConstants.DescriptionParam);
                Environment = GetParameterValue(OpenpayConfigurationConstants.EnvironmentParam);
                Username = GetParameterValue(OpenpayConfigurationConstants.Username);
                Password = GetParameterValue(OpenpayConfigurationConstants.Password);
                ExcludedProductConfigItem = GetParameterValue(OpenpayConfigurationConstants.ExcludedProductConfigItem);
                CallbackItemId = GetParameterValue(OpenpayConfigurationConstants.CallBackItemId);
                MinPurchaseLimit = GetParameterValue(OpenpayConfigurationConstants.MinPurchaseLimitParam);
                MaxPurchaseLimit = GetParameterValue(OpenpayConfigurationConstants.MaxPurchaseLimitParam);
            }
        }
        
        /// <summary>
        /// Gets the PaymentMethodDto's parameter (setting in CommerceManager of Openpay) by name.
        /// </summary>
        /// <param name="paymentMethodDto">The payment method dto.</param>
        /// <param name="parameterName">The parameter name.</param>
        /// <returns>The parameter row.</returns>
        public static PaymentMethodDto.PaymentMethodParameterRow GetParameterByName(PaymentMethodDto paymentMethodDto, string parameterName)
        {
            var rowArray = (PaymentMethodDto.PaymentMethodParameterRow[])paymentMethodDto.PaymentMethodParameter.Select(string.Format("Parameter = '{0}'", parameterName));
            return rowArray.Length > 0 ? rowArray[0] : null;
        }

        #endregion
    }
}
