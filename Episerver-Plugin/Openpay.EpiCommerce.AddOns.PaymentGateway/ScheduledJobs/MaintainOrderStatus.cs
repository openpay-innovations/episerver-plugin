using EPiServer.Commerce.Order;
using EPiServer.Logging;
using EPiServer.PlugIn;
using EPiServer.Scheduler;
using EPiServer.ServiceLocation;
using Mediachase.Commerce.Orders;
using Openpay.EpiCommerce.AddOns.PaymentGateway.Constants;
using Openpay.EpiCommerce.AddOns.PaymentGateway.Enums;
using Openpay.EpiCommerce.AddOns.PaymentGateway.Helpers;
using System;
using System.Linq;

namespace Openpay.EpiCommerce.AddOns.PaymentGateway.ScheduledJobs
{
    [ScheduledPlugIn(DisplayName = "Maintain Openpay Order Status", Description = "To maintain the correct order status at merchants as well as Openpay end", GUID = "24a6dba8-0ab8-4d5d-a869-a096d1f7ec83")]
    public class MaintainOrderStatus : ScheduledJobBase
    {
        private bool _stopSignaled;
        private readonly ILogger Logger = LogManager.GetLogger(typeof(MaintainOrderStatus));
        private readonly IPurchaseOrderProcessor purchaseOrderProcessor = ServiceLocator.Current.GetInstance<IPurchaseOrderProcessor>();
        private readonly IOrderRepository orderRepository = ServiceLocator.Current.GetInstance<IOrderRepository>();

        public MaintainOrderStatus()
        {
            IsStoppable = true;
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
            OnStatusChanged(string.Format("Starting execution of {0}", this.GetType()));

            //Get orders in pending status in Episerver Commerce & corresponding orders in Openpay
            OpenpayConfiguration openpayConfiguration = new OpenpayConfiguration();

            try
            {
                PurchaseOrder[] pendingOrders = OrderContext.Current.FindPurchaseOrdersByStatus(OrderStatus.OnHold);
                string openpayOrderId = string.Empty;
                foreach (PurchaseOrder pendingOrder in pendingOrders)
                {
                    var payment = pendingOrder.OrderForms.SelectMany(f => f.Payments).FirstOrDefault(x => x.PaymentMethodId.Equals(openpayConfiguration.PaymentMethodId));
                    if (payment != null && pendingOrder.Created < DateTime.Now.AddMinutes(-30)) // check order created by Openpay more than 30 mins ago
                    {
                        openpayOrderId = pendingOrder[OpenpayConfigurationConstants.OpenpayOrderId].ToString();
                        var orderStatusResponse = OpenpayApiHelper.GetOrderStatusApi(openpayOrderId, openpayConfiguration);
                        if (orderStatusResponse?.PlanStatus == OpenpayPlanStatus.Active.ToString())
                        {
                            // Release Episerver orders if plan is active in Openpay
                            purchaseOrderProcessor.ReleaseOrder(pendingOrder);
                            var message = $"{pendingOrder.TrackingNumber} is released";
                            Logger.Information(message);
                            Utilities.AddNoteToPurchaseOrder("RELEASE", message, pendingOrder.CustomerId, pendingOrder);
                        }
                        else
                        {
                            // Cancel Episerver orders if not active in Openpay
                            purchaseOrderProcessor.CancelOrder(pendingOrder);
                            var message = $"{pendingOrder.TrackingNumber} is cancelled";
                            Logger.Information(message);
                            Utilities.AddNoteToPurchaseOrder("CANCEL", message, pendingOrder.CustomerId, pendingOrder);
                        }

                        orderRepository.Save(pendingOrder);
                    }
                }
            }
            catch (Exception e)
            {
                return string.Format("Exception: {0} - {1}", e.Message, e.InnerException);
            }

            //For long running jobs periodically check if stop is signaled and if so stop execution
            if (_stopSignaled)
            {
                return string.Format("Stop of job {0} was called", this.GetType());
            }

            return string.Format("Job {0} was executed", this.GetType());
        }
    }
}
