using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Web.Mvc;
using EPiServer.Logging;
using Newtonsoft.Json;
using Openpay.EpiCommerce.AddOns.PaymentGateway.Constants;
using Openpay.EpiCommerce.AddOns.PaymentGateway.Enums;
using Openpay.EpiCommerce.AddOns.PaymentGateway.Models;
using Openpay.EpiCommerce.AddOns.PaymentGateway.Models.CreationNewOrderRequest;
using Openpay.EpiCommerce.AddOns.PaymentGateway.Models.CreationNewOrderResponse;


namespace Openpay.EpiCommerce.AddOns.PaymentGateway.Helpers
{
    public class OpenpayApiHelper
    {
        private static ILogger _logger
        {
            get
            {
                Func<ILoggerFactory> loggerFactory = LogManager.LoggerFactory;
                var factory = loggerFactory();
                return factory.Create(CommonConstants.OpenpayLogAppender);
            }
        }

        public static PurchaseLimits GetPurchaseLimitsApi(OpenpayConfiguration configuration)
        {
            var result = CallOpenpayGetApi<PurchaseLimits>(configuration, CommonConstants.OpenpayGetPurchaseLimitsApiEndPoint);
            result.MaxPrice /= 100;
            result.MinPrice /= 100;
            return result;
        }

        public static Order GetOrderStatusApi(string orderId, OpenpayConfiguration config, bool timeoutRetry = false)
        {
            if (string.IsNullOrWhiteSpace(orderId))
            {
                return null;
            }

            var response = CallOpenpayGetApi<Order>(config, CommonConstants.OpenpayGetOrderByIdApiEndPoint, orderId, timeoutRetry);
            return response;
        }

        public static CapturePaymentResponse CaptureOrderPayment(string epiMerchantOrderId, string openpayOrderId, OpenpayConfiguration config)
        {
            try
            {
                var endPoint = string.Format(CommonConstants.OpenpayCaptureOrderPayment, openpayOrderId);
                object bodyParam = new { retailerOrderNo = epiMerchantOrderId };
                var response = CallOpenpayPostApi<CapturePaymentResponse>(config, endPoint, param: bodyParam, retry: true);
                return response;
            }
            catch (Exception e)
            {
                return null;
            }
        }

        public static string SendRefundRequest(
            string openpayOrderId,
            int newPurchasePrice,
            int reducePriceBy,
            bool fullRefund,
            OpenpayConfiguration configuration)
        {
            var responseOrderId = CallOpenpayPostApi<OpenpayOrderId>(
                configuration,
                string.Format(CommonConstants.OpenpayRefundApiEndPoint, openpayOrderId),
                new
                {
                    newPurchasePrice,
                    reducePriceBy,
                    fullRefund
                });

            return responseOrderId?.OrderId;
        }

        public static CreationNewOrderResponse SendNewOrderCreationRequest(CreationNewOrderRequest data, OpenpayConfiguration config)
        {
            return CallOpenpayPostApi<CreationNewOrderResponse>(
                config,
                endPoint: CommonConstants.OpenpayGetOrderByIdApiEndPoint,
                param: data);
        }


        #region Private method

        private static T CallOpenpayGetApi<T>(OpenpayConfiguration config, string endPoint, string variedParam = null, bool timeoutRetry = false)
        {
            var apiUrl = GetApiUrlByRegion(config.SelectedRegion, config.SelectedEnvironment == OpenpayEnvironment.Production);
            var apiFullPath = $"{apiUrl}{endPoint}{variedParam}";

            var authToken = $"{config.Username}:{config.Password}";
            var authUsernamePassword = Encoding.ASCII.GetBytes(authToken);

            var client = new HttpClient();
            client.Timeout = TimeSpan.FromSeconds(20);
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue
            (
                "Basic",
                Convert.ToBase64String(authUsernamePassword)
            );

            // Send GET request to Openpay API / retry it 3 times if timeout error
            var timeoutRetryCount = 0;
            while (timeoutRetryCount < (timeoutRetry ? 3 : 1))
            {
                try
                {
                    LogRequest("GET", $"{apiUrl}{endPoint}", variedParam, authToken);
                    var response = client.GetAsync(apiFullPath).Result;
                    if (response.IsSuccessStatusCode)
                    {
                        var responseResult = response.Content.ReadAsStringAsync().Result;
                        return JsonConvert.DeserializeObject<T>(responseResult);
                    }

                    throw new WebException($"{response.StatusCode} - {response.RequestMessage}");
                }
                catch (WebException e)
                {
                    if (e.Status == WebExceptionStatus.Timeout)
                    {
                        // Handle timeout exception
                        timeoutRetryCount++;
                        _logger.Error($"{e.Status}. {e.Message}. {e.Response}");
                        if (timeoutRetryCount == 2)
                            throw;
                    }
                    else
                    {
                        _logger.Error($"{e.Status}. {e.Message}. {e.Response}");
                        throw;
                    }
                }
            }

            return default;
        }

        private static T CallOpenpayPostApi<T>(OpenpayConfiguration config, string endPoint, object param = null, bool retry = false)
        {
            var apiUrl = GetApiUrlByRegion(config.SelectedRegion, config.SelectedEnvironment == OpenpayEnvironment.Production);
            if (string.IsNullOrWhiteSpace(apiUrl))
            {
                return default(T);
            }
            var apiFullPath = $"{apiUrl}{endPoint}";

            var paramObject = JsonConvert.SerializeObject(param);
            var data = new StringContent(paramObject, Encoding.UTF8, "application/json");

            var authToken = $"{config.Username}:{config.Password}";
            var authUsernamePassword = Encoding.ASCII.GetBytes(authToken);
            var client = new HttpClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(
                "Basic",
                Convert.ToBase64String(authUsernamePassword));
            
            // Send GET request to Openpay API / retry it 3 times if timeout error
            var timeoutRetryCount = 0;
            while (timeoutRetryCount < (retry ? 3 : 1))
            {
                try
                {
                    LogRequest("POST", apiFullPath, paramObject, authToken);
                    var response = client.PostAsync(apiFullPath, data).Result;
                    var responseResult = response.Content.ReadAsStringAsync().Result;
                    _logger.Information($"responseResult: {responseResult}");
                    if (response.IsSuccessStatusCode)
                    {
                        var result = JsonConvert.DeserializeObject<T>(responseResult);
                        return result;
                    }

                    throw new Exception(responseResult);
                }
                catch (Exception e)
                {
                    if (e is WebException we && we.Status == WebExceptionStatus.Timeout)
                    {
                            // Handle timeout exception
                            timeoutRetryCount++;
                            _logger.Error($"{we.Status}. {we.Message}. {we.Response}");
                            if (timeoutRetryCount == 2)
                                throw;
                    }
                    else
                    {
                        _logger.Error($"{e.Message}. {e.InnerException}");
                        throw;
                    }
                }
            }

            return default;
        }

        private static void LogRequest(string method, string endPoint, string paramObject, string authentication)
        {
            var message = JsonConvert.SerializeObject(new
            {
                method,
                endPoint,
                paramObject,
                authentication
            });
            _logger.Information($"Start sending request {DateTime.Now} : {message}");
        }

        #endregion


        #region Get Url By Region

        public static string GetApiUrlByRegion(OpenpayRegion region, bool production)
        {
            switch (region)
            {
                case OpenpayRegion.AU:
                    return production ? OpenpayConfigurationConstants.AUConfiguration.Production.ApiUrl : OpenpayConfigurationConstants.AUConfiguration.Sandbox.ApiUrl;
                case OpenpayRegion.UK:
                    return production ? OpenpayConfigurationConstants.UKConfiguration.Production.ApiUrl : OpenpayConfigurationConstants.UKConfiguration.Sandbox.ApiUrl;
                case OpenpayRegion.USA:
                    return production ? OpenpayConfigurationConstants.USAConfiguration.Production.ApiUrl : OpenpayConfigurationConstants.USAConfiguration.Sandbox.ApiUrl;
                default:
                    return null;
            }
        }

        public static string GetHandoverUrlByRegion(OpenpayRegion region, bool production)
        {
            switch (region)
            {
                case OpenpayRegion.AU:
                    return production ? OpenpayConfigurationConstants.AUConfiguration.Production.HandoverUrl : OpenpayConfigurationConstants.AUConfiguration.Sandbox.HandoverUrl;
                case OpenpayRegion.UK:
                    return production ? OpenpayConfigurationConstants.UKConfiguration.Production.HandoverUrl : OpenpayConfigurationConstants.UKConfiguration.Sandbox.HandoverUrl;
                case OpenpayRegion.USA:
                    return production ? OpenpayConfigurationConstants.USAConfiguration.Production.HandoverUrl : OpenpayConfigurationConstants.USAConfiguration.Sandbox.HandoverUrl;
                default:
                    return null;
            }
        }

        #endregion


    }
}
