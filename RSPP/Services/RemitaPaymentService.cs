using log4net;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using RestSharp;
using RSPP.Helpers;
using RSPP.Models;
using RSPP.Models.DTOs.Remita;
using RSPP.Models.Options;
using RSPP.Services.Interfaces;
using System;
using System.Reflection;
using System.Threading.Tasks;

namespace RSPP.Services
{
    /// <summary>
    /// integrates remita payment services
    /// </summary>
    public class RemitaPaymentService : IRemitaPaymentService
    {
        private static readonly ILog _logger = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        private readonly RemitaOptions _remitaOptions;

        public RemitaPaymentService(IOptions<RemitaOptions> remitaOptions)
        {
            _remitaOptions = remitaOptions.Value;
        }

        /// <summary>
        /// Checks the status of a remita payment transaction
        /// </summary>
        /// <param name="rrr">Remita Retrieval Reference</param>
        /// <returns>An object representing the status of the transaction</returns>

        public async Task<RemitaTransactionResponse> CheckTransactionStatusAsync(string rrr)
        {
            var statusResponse = new RemitaTransactionResponse(false, $"{AppMessages.TRANSACTION} {AppMessages.STATUS_CHECK} {AppMessages.FAILED}");
            try
            {

                string apiHash = Encryption.GenerateSHA512($"{rrr}{_remitaOptions.ApiKey}{_remitaOptions.MerchantId}");

                var options = new RestClientOptions(_remitaOptions.BaseUrl)
                {
                    MaxTimeout = -1,
                };
                var client = new RestClient(options);
                var request = new RestRequest($"/{_remitaOptions.TransactionStatusEndpoint}/{_remitaOptions.MerchantId}/{rrr}/{apiHash}/status.reg", Method.Get);
                request.AddHeader("Content-Type", "application/json");
                request.AddHeader("Authorization", $"remitaConsumerKey={_remitaOptions.MerchantId},remitaConsumerToken={apiHash}");

                RestResponse response = await client.ExecuteAsync(request);
                if (response != null && response.IsSuccessful)
                {
                    statusResponse.RemitaResponse = JsonConvert.DeserializeObject<RemitaTransactionStatusResponse>(response.Content);
                    var statusResult = InterpretRemitaResponseStatus(statusResponse.RemitaResponse.status);
                    statusResponse.Success = statusResult.success;
                    statusResponse.ResultMessage = statusResult.message;
                }

                return statusResponse;
            }
            catch (Exception e)
            {
                statusResponse.ResultMessage = $"{AppMessages.UNABLE_TO_COMPLETE} : {AppMessages.TRANSACTION} {AppMessages.STATUS_CHECK}";
                _logger.Error(e.Message);
                return statusResponse;
            }

        }

        /// <summary>
        /// Generate RRR
        /// </summary>
        /// <param name="rrrRequest">request model to retrieve Remita Retrieval Reference</param>
        /// <returns>An object that determines if RRR was generated</returns>

        public async Task<RemitaInitiatePaymentResponse> InitiatePaymentAsync(RRRRequestModel rrrRequest)
        {
            var statusResponse = new RemitaInitiatePaymentResponse(false, $"{AppMessages.PAYMENT_REFERENCE} {AppMessages.GENERATION} {AppMessages.FAILED}");
            try
            {

                string apiHash = Encryption.GenerateSHA512($"{_remitaOptions.MerchantId}{rrrRequest.serviceTypeId}{rrrRequest.orderId}{rrrRequest.amount}{_remitaOptions.ApiKey}");

                var options = new RestClientOptions(_remitaOptions.BaseUrl)
                {
                    MaxTimeout = -1,
                };
                var client = new RestClient(options);
                var request = new RestRequest($"/{_remitaOptions.InitiatePaymentEndpoint}", Method.Post);
                request.AddHeader("Content-Type", "application/json");
                request.AddHeader("Authorization", $"remitaConsumerKey={_remitaOptions.MerchantId},remitaConsumerToken={apiHash}");
                var requestobj = JsonConvert.SerializeObject(rrrRequest);
                request.AddParameter("application/json", requestobj, ParameterType.RequestBody);

                RestResponse response = await client.ExecuteAsync(request);
                if (response != null && response.IsSuccessful)
                {
                    statusResponse.RemitaInitiatePaymentStatusResponse = JsonConvert.DeserializeObject<RemitaInitiatePaymentStatusResponse>(response.Content);
                    var statusResult = InterpretRemitaResponseStatus(statusResponse.RemitaInitiatePaymentStatusResponse.status);
                    statusResponse.Success = statusResult.success;
                    statusResponse.ResultMessage = statusResult.message;
                }

                return statusResponse;
            }
            catch (Exception e)
            {
                statusResponse.ResultMessage = $"{AppMessages.UNABLE_TO_COMPLETE} : {AppMessages.PAYMENT_REFERENCE} {AppMessages.GENERATION}";
                _logger.Error(e.Message);
                return statusResponse;
            }

        }

        /// <summary>
        /// determines remita response based on code received
        /// </summary>
        /// <param name="status">status string</param>
        /// <returns>A tuple consisting of success & message</returns>
        private static (bool success, string message) InterpretRemitaResponseStatus(string status)
        {
            bool success;
            string message;
            switch (status)
            {
                case "00":
                case "01":
                    success = true;
                    message = $"{AppMessages.TRANSACTION} {AppMessages.PAID}";
                    break;

                case "021":
                    success = true;
                    message = $"{AppMessages.TRANSACTION} {AppMessages.PENDING}";
                    break;

                case "02":
                    success = false;
                    message = $"{AppMessages.TRANSACTION} {AppMessages.FAILED}";
                    break;

                case "022":
                    success = false;
                    message = $"{AppMessages.INVALID} {AppMessages.REQUEST}";
                    break;

                case "023":
                    success = false;
                    message = $"{AppMessages.INVALID} {AppMessages.MERCHANT} or {AppMessages.ORDER}";
                    break;

                case "025":
                    success = true;
                    message = $"{AppMessages.PAYMENT_REFERENCE} {AppMessages.GENERATED}";
                    break;

                case "027":
                    success = true;
                    message = $"{AppMessages.TRANSACTION} {AppMessages.ALREADY} {AppMessages.PROCESSED}";
                    break;

                default:
                    success = false;
                    message = $"{AppMessages.TRANSACTION} {AppMessages.NOT_COMPLETE}";
                    break;
            }

            return (success, message);
        }

        
    }
}
