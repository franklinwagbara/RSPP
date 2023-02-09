using log4net;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using RestSharp;
using RSPP.Models;
using RSPP.Models.DB;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace RSPP.Helpers
{
    public class UtilityHelper : Controller
    {
        //string path = 
        public RSPPdbContext _context;
        GeneralClass generalClass = new GeneralClass();

        PaymentTransactionModel paymentRequest = new PaymentTransactionModel();
        private static ILog log = log4net.LogManager.GetLogger(typeof(UtilityHelper));

        public UtilityHelper(RSPPdbContext context)
        {
            _context = context;

        }

        public WebResponse RemitaPayment(PaymentTransactionModel model, string APIHash)
        {
            WebResponse webResponse = new WebResponse();

            var client = new RestClient(generalClass.PostPaymentUrlLive);
            client.Timeout = -1;
            var request = new RestRequest(Method.POST);
            request.AddHeader("Content-Type", "application/json");
            request.AddHeader("Authorization", "remitaConsumerKey=" + generalClass.merchantIdLive + ",remitaConsumerToken=" + APIHash);
            var requestobj = JsonConvert.SerializeObject(model);
            //StreamWriter sw = new StreamWriter("log.txt", true);
            //sw.WriteLine(requestobj);
            //sw.Dispose();

            log.Info("This is the request OBJ :" + requestobj);
            request.AddParameter("application/json", requestobj, ParameterType.RequestBody);
            IRestResponse response = client.Execute(request);
            string content = response.Content;
            var removeunwantedstring1 = content.Replace("jsonp (", "");
            var removeunwantedstring2 = removeunwantedstring1.Replace(")", "");


            if (response.IsSuccessful)
            {
                webResponse.message = "Success";
                webResponse.value = JsonConvert.DeserializeObject<RemitaModel>(removeunwantedstring2);
                StreamWriter sw_Passed = new StreamWriter("log.txt", true);
                sw_Passed.WriteLine(webResponse.message);
                sw_Passed.Dispose();
            }
            else
            {
                webResponse.message = "Failed";
                StreamWriter sw_Failed = new StreamWriter("log.txt", true);
                sw_Failed.WriteLine(webResponse.message);
                sw_Failed.Dispose();
            }
            return webResponse;
        }


        public WebResponse GetRemitaPaymentDetails(string APIHash, string rrr)
        {
            WebResponse webResponse = new WebResponse();

            var client = new RestClient(generalClass.GetPaymentBaseUrlLive+ generalClass.merchantIdLive+"/"+rrr+"/"+ APIHash+"/status.reg");
            client.Timeout = -1;
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



        public string GeneratePaymentReference(string Applicationid, string baseurl, string paymentname, decimal paymentamonut)
       {
            try
            {
                var applicationdetails = (from a in _context.ApplicationRequestForm where a.ApplicationId == Applicationid select a).FirstOrDefault();

                var agencydetails = (from a in _context.UserMaster where a.UserEmail == applicationdetails.CompanyEmail select a).FirstOrDefault();

                PaymentLog paymentLogs = _context.PaymentLog.Where(c => c.ApplicationId == Applicationid && (c.Status == "INIT" || c.Status == "AUTH" || c.Status == "FAIL")).FirstOrDefault();
                if (paymentLogs != null)
                {
                    log.Info("RRR is Already Generated =>" + paymentLogs.Rrreference);
                    return paymentLogs.Rrreference;
                }

                var servicetype = applicationdetails.ApplicationTypeId == "NEW" ? generalClass.ServiceIdNewLive : generalClass.ServiceIdRenewalLive;
                paymentRequest.serviceTypeId = servicetype;//generalClass.ServiceIdNewLive;
                paymentRequest.orderId = Applicationid;
                paymentRequest.amount = Decimal.ToInt32(paymentamonut).ToString();
                paymentRequest.payerName = agencydetails.CompanyName;
                paymentRequest.payerEmail = applicationdetails.CompanyEmail;
                paymentRequest.payerPhone = applicationdetails.PhoneNum;
                paymentRequest.description = applicationdetails.AgencyName;
                string AppkeyHash = generalClass.merchantIdLive + servicetype + Applicationid + Decimal.ToInt32(paymentamonut).ToString() + generalClass.AppKeyLive;//generalClass.merchantId + generalClass.ServiceId + Applicationid + Decimal.ToInt32(paymentamonut).ToString() + generalClass.AppKey;
                string AppkeyHashed = generalClass.GenerateSHA512(AppkeyHash).ToLower();
                WebResponse webResponse = RemitaPayment(paymentRequest, AppkeyHashed);

                RemitaModel paymentResponse = (RemitaModel)webResponse.value;
                if (webResponse.message == "Success")
                {
                    PaymentLog paymentLog = new PaymentLog();
                    paymentLog.ApplicationId = Applicationid;
                    paymentLog.TransactionDate = DateTime.UtcNow;
                    paymentLog.LastRetryDate = DateTime.UtcNow;
                    paymentLog.PaymentCategory = paymentname;
                    paymentLog.TransactionId = paymentResponse.statuscode;
                    paymentLog.ApplicantId = applicationdetails.CompanyEmail;
                    paymentLog.Rrreference = paymentResponse.RRR;
                    paymentLog.AppReceiptId = "APPID";
                    paymentLog.TxnAmount = paymentamonut;
                    paymentLog.Arrears = 0;
                    paymentLog.TxnMessage = paymentResponse.status;
                    paymentLog.Account = generalClass.AccountNumberLive;
                    paymentLog.BankCode = generalClass.BankCodeLive;
                    paymentLog.RetryCount = 0;
                    paymentLog.Status = "INIT";

                    log.Info("About to Add Payment Log");
                    _context.PaymentLog.Add(paymentLog);

                    log.Info("Added Payment Log to Table");
                    _context.SaveChanges();
                    log.Info("Saved it Successfully");
                }
                    return paymentResponse.RRR;
                
            }
            catch (Exception ex)
            {
                log.Error(ex.StackTrace);
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
