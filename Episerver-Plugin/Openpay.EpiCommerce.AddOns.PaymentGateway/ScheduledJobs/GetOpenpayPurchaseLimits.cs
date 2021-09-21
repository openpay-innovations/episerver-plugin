using EPiServer.PlugIn;
using EPiServer.Scheduler;
using Mediachase.Commerce.Orders.Dto;
using Mediachase.Commerce.Orders.Managers;
using Openpay.EpiCommerce.AddOns.PaymentGateway.Constants;
using Openpay.EpiCommerce.AddOns.PaymentGateway.Enums;
using Openpay.EpiCommerce.AddOns.PaymentGateway.Helpers;
using System;
using System.Linq;
using EPiServer;
using EPiServer.Core;
using EPiServer.DataAbstraction;
using EPiServer.DataAccess;
using EPiServer.Logging;
using EPiServer.Security;
using Openpay.EpiCommerce.AddOns.PaymentGateway.BlockTypes;

namespace Openpay.EpiCommerce.AddOns.PaymentGateway.ScheduledJobs
{
    [ScheduledPlugIn(
        DisplayName = "Openpay Get Purchase Limits", 
        Description = "Get the minimum and maximum price range from Openpay", 
        GUID = "91dcd915-01ac-409a-ba02-3754d688df9a")]
    public class GetOpenpayPurchaseLimits : ScheduledJobBase
    {
        private bool _stopSignaled;
        private IContentRepository _contentRepo;
        private readonly ILogger Logger = LogManager.GetLogger(typeof(GetOpenpayPurchaseLimits));

        public GetOpenpayPurchaseLimits(IContentRepository contentRepo)
        {
            IsStoppable = true;
            _contentRepo = contentRepo;
        }

        /// <summary>
        /// Called when a user clicks on Stop for a manually started job, or when ASP.NET shuts down.
        /// </summary>
        public override void Stop()
        {
            _stopSignaled = true;
        }

        /// <summary>
        /// Called when a scheduled job executes
        /// </summary>
        /// <returns>A status message to be stored in the database log and visible from admin mode</returns>
        public override string Execute()
        {
            //Call OnStatusChanged to periodically notify progress of job for manually started jobs
            OnStatusChanged(String.Format("Starting execution of {0}", this.GetType()));

            var message = String.Format("Job {0} was executed", this.GetType());
            //Get Openpay Purchase Limits from API then save into configuration
            OpenpayConfiguration openpayConfiguration = new OpenpayConfiguration();
            try
            {
                var purchaseLimits = OpenpayApiHelper.GetPurchaseLimitsApi(openpayConfiguration);
                if (purchaseLimits != null)
                {
                    PaymentMethodDto paymentMethodDto = OpenpayConfiguration.GetOpenpayPaymentMethod();
                    if (paymentMethodDto != null && paymentMethodDto.PaymentMethodParameter != null)
                    {

                        var minPurchaseLimitParam = OpenpayConfiguration.GetParameterByName(paymentMethodDto, OpenpayConfigurationConstants.MinPurchaseLimitParam);
                        var maxPurchaseLimitParam = OpenpayConfiguration.GetParameterByName(paymentMethodDto, OpenpayConfigurationConstants.MaxPurchaseLimitParam);
                        if (minPurchaseLimitParam == null)
                        {
                            return "Openpay min purchase limit not setup";
                        }
                        if (maxPurchaseLimitParam == null)
                        {
                            return "Openpay max purchase limit not setup";
                        }
                        // save to openpay configuration
                        minPurchaseLimitParam.Value = purchaseLimits.MinPrice.ToString();
                        maxPurchaseLimitParam.Value = purchaseLimits.MaxPrice.ToString();
                        PaymentManager.SavePayment(paymentMethodDto);

                        #region save to openpay widget configuration blocks

                        var configBlock = WidgetViewModelFactory.GetConfigurationBlock().CreateWritableClone() as OpenpayWidgetConfigurationBlock;
                        if (configBlock == null)
                        {
                            return message;
                        }
                        configBlock.MinEligibleAmount = purchaseLimits.MinPrice;
                        configBlock.MaxEligibleAmount = purchaseLimits.MaxPrice;
                        _contentRepo.Save((IContent)configBlock, SaveAction.Publish, AccessLevel.Edit);

                        #endregion
                    }
                }
            }
            catch (Exception e)
            {
                Logger.Error(e.Message +  e.Source);
                return String.Format("Unable to save Openpay payment purchase limit, exception: {0} - {1}", e.Message, e.InnerException);
            }

            //For long running jobs periodically check if stop is signaled and if so stop execution
            if (_stopSignaled)
            {
                return String.Format("Stop of job {0} was called", this.GetType());
            }

            return message;
        }
    }
}
