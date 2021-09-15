using EPiServer.DataAbstraction;
using EPiServer.ServiceLocation;
using Mediachase.Commerce.Orders.Dto;
using Mediachase.Web.Console.Interfaces;
using Openpay.EpiCommerce.AddOns.PaymentGateway.Helpers;
using System;
using System.Collections.Generic;
using System.Data;
using System.Web.UI.WebControls;
using EPiServer.Logging;
using Openpay.EpiCommerce.AddOns.PaymentGateway.Constants;
using Openpay.EpiCommerce.AddOns.PaymentGateway.Enums;
using Openpay.EpiCommerce.AddOns.PaymentGateway.Models;

namespace Openpay.EpiCommerce.AddOns.PaymentGateway.CommerceManager
{
    public partial class ConfigurePayment : System.Web.UI.UserControl, IGatewayControl
    {
        private PaymentMethodDto PaymentMethodDto { get; set; }
        private readonly ILogger Logger = LogManager.GetLogger(typeof(ConfigurePayment));

        public string ValidationGroup { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="ConfigurePayment"/> class.
        /// </summary>
        public ConfigurePayment()
        {
            ValidationGroup = string.Empty;
            PaymentMethodDto = null;
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                BindData();
            }
        }

        /// <summary>
        /// Loads the PaymentMethodDto object.
        /// </summary>
        /// <param name="dto">The PaymentMethodDto object.</param>
        public void LoadObject(object dto)
        {
            PaymentMethodDto = dto as PaymentMethodDto;
        }

        /// <summary>
        /// Saves the changes.
        /// </summary>
        /// <param name="dto">The dto.</param>
        public void SaveChanges(object dto)
        {
            // call openpay api to validate username & password
            try
            {
                CallPurchaseLimitsApi();
            }
            catch (Exception e)
            {
                ErrorMessage.Text = $"\n Incorrect credential, please check your username/password !";
                ErrorMessage.Text += $"\n Error: {e.Message}";
                return;
            }

            if (!Visible)
            {
                return;
            }

            PaymentMethodDto = dto as PaymentMethodDto;
            if (PaymentMethodDto != null && PaymentMethodDto.PaymentMethodParameter != null)
            {
                try
                {
                    var paymentMethodId = Guid.Empty;
                    if (PaymentMethodDto.PaymentMethod.Count > 0)
                    {
                        paymentMethodId = PaymentMethodDto.PaymentMethod[0].PaymentMethodId;
                    }

                    UpdateOrCreateParameter(OpenpayConfigurationConstants.RegionParam, Region, paymentMethodId);
                    UpdateOrCreateParameter(OpenpayConfigurationConstants.DescriptionParam, Description, paymentMethodId);
                    UpdateOrCreateParameter(OpenpayConfigurationConstants.EnvironmentParam, Environment, paymentMethodId);
                    UpdateOrCreateParameter(OpenpayConfigurationConstants.Username, Username, paymentMethodId);
                    UpdateOrCreateParameter(OpenpayConfigurationConstants.Password, Password, paymentMethodId);
                    UpdateOrCreateParameter(OpenpayConfigurationConstants.ExcludedProductConfigItem, ExcludedProductConfigItem, paymentMethodId);
                    UpdateOrCreateParameter(OpenpayConfigurationConstants.CallBackItemId, CallBackItemId, paymentMethodId);
                    UpdateOrCreateParameter(OpenpayConfigurationConstants.MinPurchaseLimitParam, MinPurchaseLimit, paymentMethodId);
                    UpdateOrCreateParameter(OpenpayConfigurationConstants.MaxPurchaseLimitParam, MaxPurchaseLimit, paymentMethodId);
                }
                catch (Exception e)
                {
                    Logger.Error($"{e.Message} - {e.StackTrace}");
                    ErrorMessage.Text = $"{e.Message} - {e.StackTrace}";
                }
            }
        }


        #region Private Method

        /// <summary>
        /// Binds the data.
        /// </summary>
        private void BindData()
        {
            if (PaymentMethodDto != null && PaymentMethodDto.PaymentMethodParameter != null)
            {
                // Bind region data
                Region.DataTextField = "Text";
                Region.DataValueField = "Value";
                Region.DataSource = OpenpayConfigurationConstants.RegionListItems;
                Region.DataBind();
                // Bind environment data
                Environment.DataTextField = "Text";
                Environment.DataValueField = "Value";
                Environment.DataSource = OpenpayConfigurationConstants.EnvironmentListItems;
                Environment.DataBind();

                BindParameterData(OpenpayConfigurationConstants.RegionParam, Region);
                BindParameterData(OpenpayConfigurationConstants.DescriptionParam, Description);
                BindParameterData(OpenpayConfigurationConstants.EnvironmentParam, Environment);
                BindParameterData(OpenpayConfigurationConstants.Username, Username);
                BindParameterData(OpenpayConfigurationConstants.Password, Password);
                BindParameterData(OpenpayConfigurationConstants.ExcludedProductConfigItem, ExcludedProductConfigItem);
                BindParameterData(OpenpayConfigurationConstants.CallBackItemId, CallBackItemId);
                var minPurchase = GetParameterByName(OpenpayConfigurationConstants.MinPurchaseLimitParam);
                MinPurchaseLimit.Text = minPurchase != null ? minPurchase.Value : "0";
                var maxPurchase = GetParameterByName(OpenpayConfigurationConstants.MaxPurchaseLimitParam);
                MaxPurchaseLimit.Text = maxPurchase != null ? maxPurchase.Value : "0";
            }
            else
            {
                Visible = false;
            }
        }

        public void RunPurchaseLimitsAPI(object sender, EventArgs eventArgs)
        {
            try
            {
                var purchaseLimits = CallPurchaseLimitsApi();
                if (purchaseLimits != null)
                {
                    this.MinPurchaseLimit.Text = purchaseLimits.MinPrice.ToString();
                    this.MaxPurchaseLimit.Text = purchaseLimits.MaxPrice.ToString();
                    ErrorMessage.Text = $"Fetch Purchase Limits successful! Remember to save changes.";

                    return;
                }
                ErrorMessage.Text = $"\n Your region value is not correct";
            }
            catch (Exception e)
            {
                ErrorMessage.Text = $"\n {e.Message}";
                Logger.Error($"{e.Message} - {e.StackTrace}");
            }
        }

        private PurchaseLimits CallPurchaseLimitsApi()
        {
            var currentSettings = new Dictionary<string, string>
            {
                { OpenpayConfigurationConstants.RegionParam, Region.SelectedValue},
                { OpenpayConfigurationConstants.EnvironmentParam, Environment.SelectedValue},
                { OpenpayConfigurationConstants.Username, Username.Text},
                { OpenpayConfigurationConstants.Password, Password.Text}
            };
            var openpayConfiguration = new OpenpayConfiguration(currentSettings);
            return OpenpayApiHelper.GetPurchaseLimitsApi(openpayConfiguration);
        }

        private void UpdateOrCreateParameter(string parameterName, TextBox parameterControl, Guid paymentMethodId)
        {
            var parameter = GetParameterByName(parameterName);
            if (parameter != null)
            {
                parameter.Value = parameterControl.Text;
            }
            else
            {
                var row = PaymentMethodDto.PaymentMethodParameter.NewPaymentMethodParameterRow();
                row.PaymentMethodId = paymentMethodId;
                row.Parameter = parameterName;
                row.Value = parameterControl.Text;
                PaymentMethodDto.PaymentMethodParameter.Rows.Add(row);
            }
        }

        private void UpdateOrCreateParameter(string parameterName, DropDownList parameterControl, Guid paymentMethodId)
        {
            var parameter = GetParameterByName(parameterName);
            var value = parameterControl.SelectedValue;
            if (parameter != null)
            {
                parameter.Value = value;
            }
            else
            {
                var row = PaymentMethodDto.PaymentMethodParameter.NewPaymentMethodParameterRow();
                row.PaymentMethodId = paymentMethodId;
                row.Parameter = parameterName;
                row.Value = value;
                PaymentMethodDto.PaymentMethodParameter.Rows.Add(row);
            }
        }

        private void UpdateOrCreateParameter(string parameterName, RadioButtonList parameterControl, Guid paymentMethodId)
        {
            var parameter = GetParameterByName(parameterName);
            var value = parameterControl.SelectedValue;
            if (parameter != null)
            {
                parameter.Value = value;
            }
            else
            {
                var row = PaymentMethodDto.PaymentMethodParameter.NewPaymentMethodParameterRow();
                row.PaymentMethodId = paymentMethodId;
                row.Parameter = parameterName;
                row.Value = value;
                PaymentMethodDto.PaymentMethodParameter.Rows.Add(row);
            }
        }


        private void BindParameterData(string parameterName, TextBox parameterControl)
        {
            var parameterByName = GetParameterByName(parameterName);
            if (parameterByName != null)
            {
                parameterControl.Text = parameterByName.Value;
            }
        }

        private void BindParameterData(string parameterName, DropDownList parameterControl)
        {
            var parameterByName = GetParameterByName(parameterName);
            if (parameterByName != null)
            {
                parameterControl.SelectedValue = parameterByName.Value;
            }
        }

        private void BindParameterData(string parameterName, RadioButtonList parameterControl)
        {
            var parameterByName = GetParameterByName(parameterName);
            if (parameterByName != null)
            {
                parameterControl.SelectedValue = parameterByName.Value;
            }
        }

        private PaymentMethodDto.PaymentMethodParameterRow GetParameterByName(string name)
        {
            return OpenpayConfiguration.GetParameterByName(PaymentMethodDto, name);
        }

        #endregion

    }
}