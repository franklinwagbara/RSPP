using log4net;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using RestSharp;
using RSPP.Helpers.SerilogService.GeneralLogs;
using RSPP.Models;
using RSPP.Models.DB;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Security;
using System.Security.Authentication;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

namespace RSPP.Helpers
{
    public class UtilityHelper : Controller
    {

        public RSPPdbContext _context;
        GeneralClass generalClass = new GeneralClass();
        private readonly GeneralLogger _generalLogger;
        private readonly IHttpClientFactory _clientFactory;
        PaymentTransactionModel paymentRequest = new PaymentTransactionModel();
        private readonly string directory = "PaymentLogs";
        public UtilityHelper(RSPPdbContext context, GeneralLogger generalLogger, IHttpClientFactory clientFactory)
        {
            _context = context;
            _generalLogger = generalLogger;
            _clientFactory = clientFactory;

        }

        public async Task<WebResponse> RemitaPayment(PaymentTransactionModel model, string APIHash)
        {

            WebResponse webResponse = new WebResponse();
            System.Net.WebClient _httpClient = new System.Net.WebClient();
            //System.Net.ServicePointManager.ServerCertificateValidationCallback += (sender, certificate, chain, errors) =>
            //{
            //    return errors == SslPolicyErrors.None;
            //}; 

            //var client = _clientFactory.CreateClient("HttpClientWithSSLUntrusted");
            //HttpClientHandler handler = new HttpClientHandler();
            //HttpClient client = new HttpClient(handler);



            var requestobj = JsonConvert.SerializeObject(model);

            _httpClient.Headers.Add(System.Net.HttpRequestHeader.ContentType, "application/json");

            _httpClient.Headers.Add(System.Net.HttpRequestHeader.Authorization, "remitaConsumerKey=" + generalClass.merchantIdLive + ",remitaConsumerToken=" + APIHash);

            System.Net.ServicePointManager.ServerCertificateValidationCallback =
          new RemoteCertificateValidationCallback(
        delegate (object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors errors)
        {
            return true;
        });
            string response = _httpClient.UploadString(generalClass.PostPaymentUrlLive, "POST", requestobj);
            string responseBody = response;
            responseBody = responseBody.Replace("jsonp (", "");
            responseBody = responseBody.Replace(")", "");
            if (response.Contains("RRR"))
            {
                webResponse.message = "Success";
                webResponse.value = JsonConvert.DeserializeObject<RemitaModel>(responseBody);
                _generalLogger.LogRequest($"{"RRR was succesfully genereted => "} {webResponse.value}{"=>"}{DateTime.Now}", false, directory);

            }
            else if (response.Contains("DUPLICATE"))
            {
                var message = JsonConvert.DeserializeObject<RemitaErrorResponseModel>(response);
                _generalLogger.LogRequest($"{"Duplicate error occurred => "} {message}{"=>"}{DateTime.Now}", false, directory);
                webResponse.message = "failed " + "Response Status => " + message.status + " | Response Message =>" + message.statusMessage;
            }
            else
            {
                webResponse.message = "Failed Remita Response => " + response;
                _generalLogger.LogRequest($"{"Failed Remita Response => "} {response}{"=>"}{DateTime.Now}", false, directory);

            }
            return webResponse;
        }








        public WebResponse GetRemitaPaymentDetails(string APIHash, string rrr)
        {
            WebResponse webResponse = new WebResponse();

            var client = new RestClient(generalClass.GetPaymentBaseUrlLive + generalClass.merchantIdLive + "/" + rrr + "/" + APIHash + "/status.reg");
            client.Timeout = -1;
            System.Net.ServicePointManager.ServerCertificateValidationCallback =
           new RemoteCertificateValidationCallback(
         delegate (object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors errors)
         {
             return true;
         });
            var request = new RestRequest(Method.GET);
            request.AddHeader("Content-Type", "application/json");
            request.AddHeader("Authorization", "remitaConsumerKey=" + generalClass.merchantIdLive + ",remitaConsumerToken=" + APIHash);
            IRestResponse response = client.Execute(request);
            if (response.IsSuccessful)
            {
                webResponse.message = "Success";
                webResponse.value = JsonConvert.DeserializeObject<GetPaymentResponse>(response.Content);
            }
            else
            {
                webResponse.message = "Failed";
            }
            return webResponse;
        }



        public async Task<string> GeneratePaymentReference(string Applicationid, string baseurl, string paymentname, decimal paymentamonut)
        {
            try
            {
                var applicationdetails = (from a in _context.ApplicationRequestForm where a.ApplicationId == Applicationid select a).FirstOrDefault();

                var agencydetails = (from a in _context.UserMaster where a.UserEmail == applicationdetails.CompanyEmail select a).FirstOrDefault();
                _generalLogger.LogRequest($"{"got company details"}{"-"}{DateTime.Now}", false, directory);

                PaymentLog paymentLogs = _context.PaymentLog.Where(c => c.ApplicationId == Applicationid && (c.Status == "INIT" || c.Status == "AUTH" || c.Status == "FAIL")).FirstOrDefault();
                if (paymentLogs != null)
                {
                    _generalLogger.LogRequest($"{"RRR is Already Generated =>"}{paymentLogs.Rrreference}{"-"}{DateTime.Now}", false, directory);

                    return paymentLogs.Rrreference;
                }
                _generalLogger.LogRequest($"{"About to generate RRR"}{"-"}{DateTime.Now}", false, directory);

                var servicetype = applicationdetails.ApplicationTypeId == "NEW" ? generalClass.ServiceIdNewLive : generalClass.ServiceIdRenewalLive;
                paymentRequest.serviceTypeId = servicetype;//generalClass.ServiceIdNewLive;
                paymentRequest.orderId = Applicationid;
                paymentRequest.amount = Decimal.ToInt32(paymentamonut).ToString();
                paymentRequest.payerName = agencydetails.CompanyName;
                paymentRequest.payerEmail = applicationdetails?.CompanyEmail;
                paymentRequest.payerPhone = applicationdetails?.PhoneNum;
                paymentRequest.description = applicationdetails?.AgencyName;
                string AppkeyHash = generalClass.merchantIdLive + servicetype + Applicationid + Decimal.ToInt32(paymentamonut).ToString() + generalClass.AppKeyLive;//generalClass.merchantId + generalClass.ServiceId + Applicationid + Decimal.ToInt32(paymentamonut).ToString() + generalClass.AppKey;
                string AppkeyHashed = generalClass.GenerateSHA512(AppkeyHash).ToLower();
                WebResponse webResponse = await RemitaPayment(paymentRequest, AppkeyHashed);
                _generalLogger.LogRequest($"{"Remita web response value =>"} {webResponse.value} {"Remita web response message =>"} {webResponse.message}{"-"}{DateTime.Now}", false, directory);

                RemitaModel paymentResponse = (RemitaModel)webResponse.value;
                _generalLogger.LogRequest($"{"Remita endpoint response =>"} {"RRR =>"} {paymentResponse.RRR} {"RRR statuscode =>"}{paymentResponse.statuscode}{"RRR status =>"}{paymentResponse.status}{"-"}{DateTime.Now}", false, directory);

                if (webResponse.message == "Success")
                {
                    _generalLogger.LogRequest($"{"Remita endpoint successful =>"} {paymentResponse.RRR}{"-"}{DateTime.Now}", false, directory);

                    PaymentLog paymentLog = new PaymentLog();
                    paymentLog.ApplicationId = Applicationid;
                    paymentLog.TransactionDate = DateTime.UtcNow;
                    paymentLog.LastRetryDate = DateTime.UtcNow;
                    paymentLog.PaymentCategory = paymentname;
                    paymentLog.TransactionId = paymentResponse.statuscode;
                    paymentLog.ApplicantId = applicationdetails?.CompanyEmail;
                    paymentLog.Rrreference = paymentResponse.RRR;
                    paymentLog.AppReceiptId = "APPID";
                    paymentLog.TxnAmount = paymentamonut;
                    paymentLog.Arrears = 0;
                    paymentLog.TxnMessage = paymentResponse.status;
                    paymentLog.Account = generalClass.AccountNumberLive;
                    paymentLog.BankCode = generalClass.BankCodeLive;
                    paymentLog.RetryCount = 0;
                    paymentLog.Status = "INIT";
                    _generalLogger.LogRequest($"{"About to Add Payment Log with RRR"} {paymentResponse.RRR}{"-"}{DateTime.Now}", false, directory);
                    _context.PaymentLog.Add(paymentLog);
                    _generalLogger.LogRequest($"{"Added Payment Log to Table with RRR"} {paymentResponse.RRR}{"-"}{DateTime.Now}", false, directory);
                    _context.SaveChanges();
                    _generalLogger.LogRequest($"{"Payment Log was successfully saved with RRR"} {paymentResponse.RRR}{"-"}{DateTime.Now}", false, directory);

                }
                return paymentResponse.RRR;

            }
            catch (Exception ex)
            {
                _generalLogger.LogRequest($"{"An Error Occured Generating Payment Reference, Pls Try again Later"}{ex}{"-"}{DateTime.Now}", true, directory);

                return "An Error Occured Generating Payment Reference, Pls Try again Later";
            }

        }



        public LineOfBusiness Fees(int Categoryid)
        {
            var details = (from a in _context.LineOfBusiness where a.LineOfBusinessId == Categoryid select a).FirstOrDefault();
            return details;
        }


    }
}
