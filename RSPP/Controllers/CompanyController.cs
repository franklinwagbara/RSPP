using log4net;
using RSPP.Helper;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using RSPP.Configurations;
using RSPP.Helpers;
using RSPP.Models;
using RSPP.Models.DB;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Globalization;
using System.Net.Http.Headers;
using RSPP.Helpers.SerilogService.GeneralLogs;
using System.Net.Http;

namespace RSPP.Controllers
{
    public class CompanyController : Controller
    {
        public RSPPdbContext _context;
        WorkFlowHelper _workflowHelper;
        IHttpContextAccessor _httpContextAccessor;
        public IConfiguration _configuration;
        GeneralClass generalClass = new GeneralClass();
        ResponseWrapper responseWrapper = new ResponseWrapper();
        List<ApplicationRequestForm> AppRequest = null;
        HelperController _helpersController;
        UtilityHelper _utilityHelper;
        private ILog log = log4net.LogManager.GetLogger(typeof(CompanyController));
        private readonly string directory = "CompanyLogs";
        private readonly GeneralLogger _generalLogger;

        [Obsolete]
        private readonly IHostingEnvironment _hostingEnv;


        [Obsolete]
        public CompanyController(RSPPdbContext context, IConfiguration configuration, GeneralLogger generalLogger, IHttpClientFactory clientFactory, IHttpContextAccessor httpContextAccessor, IHostingEnvironment hostingEnv)
        {
            _context = context;
            _configuration = configuration;
            _httpContextAccessor = httpContextAccessor;
            _hostingEnv = hostingEnv;
            _generalLogger = generalLogger;
            _helpersController = new HelperController(_context, _configuration, _httpContextAccessor);
            _workflowHelper = new WorkFlowHelper(_context);
            _utilityHelper = new UtilityHelper(_context, generalLogger, clientFactory);
        }


        public IActionResult Index()
        {
            string responseMessage = null;
            Dictionary<string, int> appStatistics;
            Dictionary<string, int> appStageReference = null;
            var companyemail = _helpersController.getSessionEmail();
            try
            {
                _generalLogger.LogRequest($"{"About To Generate User DashBoard Information"}{"-"}{DateTime.Now}", false, directory);

                ViewBag.TotalPermitCount = 0;
                ViewBag.ApplicationCount = 0;
                ViewBag.PermitExpiringCount = 0;
                ViewBag.ProcessedApplicationCount = 0;
                ViewBag.CompanyName = _helpersController.getSessionCompanyName();

                var Rejectcomment = (from u in _context.UserMaster
                                     where u.UserEmail == _helpersController.getSessionEmail()
                                     join a in _context.ApplicationRequestForm on u.UserEmail equals a.CompanyEmail
                                     join ah in _context.ActionHistory on a.ApplicationId equals ah.ApplicationId
                                     orderby ah.ActionId descending
                                     select new { ah.Message, ah.Action }).FirstOrDefault();



                if (Rejectcomment != null)
                {
                    TempData["Rejectcomment"] = Rejectcomment.Message;
                    TempData["Acceptcomment"] = Rejectcomment.Action;
                }
                AppRequest = _helpersController.GetApplicationDetails(companyemail, out AppRequest);

                ViewBag.AllApplicationStageDetails = AppRequest;


                var extrapay = (from a in _context.ApplicationRequestForm
                                join e in _context.ExtraPayment on a.ApplicationId equals e.ApplicationId
                                where a.ApplicationId == e.ApplicationId && e.Status != "AUTH"
                                select new { e.TxnAmount, a.ApplicationId, a.CompanyEmail }).ToList().LastOrDefault();
                if (extrapay != null)
                {
                    ViewBag.ExtraPaymentAmount = extrapay.TxnAmount;
                    ViewBag.ExtraPaymentAPPID = extrapay.ApplicationId;
                    ViewBag.ExtraPay = extrapay;
                    ViewBag.LoggedInUser = _helpersController.getSessionEmail();
                    ViewBag.ExtraPaymentEmail = extrapay.CompanyEmail;
                }




                _generalLogger.LogRequest($"{"About To Get Applications and Company Notification Messages"}{"-"}{DateTime.Now}", false, directory);

                var userMaster = (from u in _context.UserMaster where u.UserEmail == companyemail select u).FirstOrDefault();

                ViewBag.AllMessages = _helpersController.GetCompanyMessages(_context, userMaster);

                ViewBag.AllUserApplication = _helpersController.GetApplications(userMaster.UserEmail, "ALL", out responseMessage);

                _generalLogger.LogRequest($"{"GetApplications ResponseMessage => " + responseMessage}{"-"}{DateTime.Now}", false, directory);

                if (responseMessage == "SUCCESS")
                {

                    ViewBag.Allcomments = _helpersController.AllHistoryComment(_helpersController.getSessionEmail());

                    appStatistics = _helpersController.GetApplicationStatistics(_helpersController.getSessionEmail(), out responseMessage, out appStageReference);
                    ViewBag.TotalPermitCount = appStatistics["PEM"];
                    ViewBag.ApplicationCount = appStatistics["ALL"];
                    ViewBag.PermitExpiringCount = appStatistics["EXP"];
                    ViewBag.ProcessedApplicationCount = appStatistics["PROC"];

                    //ViewBag.AllUserApplication = _helpersController.GetApplications(_helpersController.getSessionEmail(), "ALL", out responseMessage);
                    //ViewBag.AllUserApplication = _helpersController.GetUnissuedLicense(_helpersController.getSessionEmail(), "ALL", out responseMessage);
                    _generalLogger.LogRequest($"{"GetApplicationStatistics ResponseMessage => " + responseMessage}{"-"}{DateTime.Now}", false, directory);

                }

                _generalLogger.LogRequest($"{"GetApplicationStatistics Count => " + appStageReference.Count}{"-"}{DateTime.Now}", false, directory);

                ViewBag.StageReferenceList = appStageReference;
                ViewBag.ErrorMessage = responseMessage;

            }
            catch (Exception ex)
            {
                _generalLogger.LogRequest($"{ex.StackTrace + Environment.NewLine + "InnerException =>" + ex.InnerException}{"-"}{DateTime.Now}", true, directory);

                ViewBag.ErrorMessage = "Error Occured Getting the Company DashBoard, Please Try again Later";
            }

            return View();
        }


        public IActionResult ApplicationForm(string ApplicationId)
        {
            var appdetailvalues = _helpersController.ApplicationDetails(ApplicationId);
            ViewBag.AgencyEmail = _helpersController.getSessionEmail();

            return View(appdetailvalues);
        }


        [HttpPost]
        public JsonResult ApplicationForm(MyApplicationRequestForm model, List<Terminal> MyTerminals)
        {
            string status = string.Empty;
            string message = string.Empty;
            var companyemail = _helpersController.getSessionEmail();
            var generatedapplicationid = generalClass.GenerateApplicationNo();
            var checkappexist = (from a in _context.ApplicationRequestForm where a.ApplicationId == model.ApplicationId select a).FirstOrDefault();
            var appid = checkappexist == null ? generatedapplicationid : model.ApplicationId;
         
            status =  _helpersController.ApplicationForm(model, MyTerminals, "NO", appid);

            if(status == "Rejected")
            {
                _generalLogger.LogRequest($"{"Application Form -- application status is rejected"}{"-"}{DateTime.Now}", false, directory);

                responseWrapper = _workflowHelper.processAction(appid, "GenerateRRR", companyemail, "Payment recieved");
                if (responseWrapper.status == true)
                {
                    _generalLogger.LogRequest($"{"Application Form -- application resubmitted"}{"-"}{DateTime.Now}", false, directory);

                    return Json(new { Status = "resubmit", applicationId = appid, Message = responseWrapper.value });

                }
                _generalLogger.LogRequest($"{"Application Form -- Unable to resubmit your application"}{"-"}{DateTime.Now}", false, directory);

                return Json(new { Status = status, applicationId = appid, Message = "Unable to resubmit your application please try again later." });
            }
            responseWrapper = _workflowHelper.processAction(appid, "Proceed", companyemail, "Initiated Application");
            if (responseWrapper.status == true)
            {
                _generalLogger.LogRequest($"{"Application Form -- "+ responseWrapper.value}{"-"}{DateTime.Now}", false, directory);

                return Json(new { Status = "success", applicationId = appid, Message = responseWrapper.value });

            }
            _generalLogger.LogRequest($"{"Application Form -- Unable to submit your application"}{"-"}{DateTime.Now}", false, directory);

            return Json(new { Status = status, applicationId = appid, Message = "Unable to submit your application please try again later." });
           
        }









        [HttpGet]
        public ActionResult MyApplications()
        {
            List<ApplicationRequestForm> apps = new List<ApplicationRequestForm>();
            try
            {
                apps = (from p in _context.ApplicationRequestForm.AsEnumerable()
                             where p.CompanyEmail == _helpersController.getSessionEmail() && p.IsLegacy == "NO"
                             select new ApplicationRequestForm
                             {
                                 ApplicationId = p.ApplicationId,
                                 CompanyEmail = p.CompanyEmail,
                                 AgencyName = p.AgencyName,
                                 AddedDate = p.AddedDate,
                                 Status = p.Status,
                                 CurrentStageId = p.CurrentStageId
                             }).ToList();

            }
            catch (Exception ex)
            {
                _generalLogger.LogRequest($"{"My Application -- Error Occured Getting  Application List"} {ex}{"-"}{DateTime.Now}", true, directory);

                ViewBag.ResponseMessage = "Error Occured Getting  Application List, Please Try Again Later";
            }
            return View(apps);
        }



        [HttpGet]
        public ActionResult MyLegacyApplications()
        {
           List <ApplicationRequestForm> legacyapp = new List<ApplicationRequestForm>();
            try
            {
                 legacyapp = (from p in _context.ApplicationRequestForm.AsEnumerable() 
                                  where p.CompanyEmail == _helpersController.getSessionEmail() && p.IsLegacy == "YES"
                                  select new ApplicationRequestForm
                                  {
                                      ApplicationId = p.ApplicationId,
                                      CompanyEmail = p.CompanyEmail,
                                      AgencyName = p.AgencyName,
                                      AddedDate = p.AddedDate,
                                      Status = p.Status,
                                      LicenseReference =p.LicenseReference,
                                      CurrentStageId = p.CurrentStageId
                                  }).ToList();
              
            }
            catch (Exception ex)
            {
                _generalLogger.LogRequest($"{"MyLegacyApplications -- Error Occured Getting Legacy Application List"} {ex}{"-"}{DateTime.Now}", true, directory);

                ViewBag.ResponseMessage = "Error Occured Getting Legacy Application List, Please Try Again Later";
            }
            return View(legacyapp);
        }





        [HttpPost]
        public JsonResult DeleteApplication(string AppID)
        {
            string message = "";
            try
            {
                var deleteapp = (from a in _context.ApplicationRequestForm where a.ApplicationId == AppID select a).FirstOrDefault();

                if (deleteapp != null)
                {

                    _context.ApplicationRequestForm.Remove(deleteapp);

                }
                _generalLogger.LogRequest($"{"DeleteApplication -- An application with reference number "+AppID+" was succesfully deleted"}{"-"}{DateTime.Now}", false, directory);

                message = "success";
                _context.SaveChanges();
            }
            catch (Exception ex) {
                _generalLogger.LogRequest($"{"DeleteApplication -- Error Occured while trying to delete application"} {ex}{"-"}{DateTime.Now}", true, directory);

                message = ex.Message; 
            }

            return Json(message);
        }






        [HttpGet]
        public ActionResult ChargeSummary(string RRR, string applicationId, decimal amount)
        {

            var applicationdetails = (from a in _context.ApplicationRequestForm where a.ApplicationId == applicationId select a).FirstOrDefault();
            var paymentdetails = (from a in _context.PaymentLog where a.ApplicationId == applicationId select a).FirstOrDefault();
           var RemitaRef= paymentdetails != null ? paymentdetails.Rrreference : RRR;
            string APIHash = generalClass.merchantIdLive + RemitaRef + generalClass.AppKeyLive; //generalClass.merchantId + RRR + generalClass.AppKey;
            ViewBag.AppkeyHashed = generalClass.GenerateSHA512(APIHash).ToLower();
            ViewBag.AgencyName = applicationdetails.AgencyName;
            ViewBag.Applicationid = applicationId;
            ViewBag.RRR = RemitaRef;
            ViewBag.Amount = paymentdetails != null ? paymentdetails.TxnAmount: amount;
            ViewBag.MerchantId = generalClass.merchantIdLive;//generalClass.merchantId;
            ViewBag.BaseUrl = generalClass.PortalBaseUrlLive;//generalClass.PortalBaseUrl;

            return View();
        }



        [HttpGet]
        public ActionResult PaymentReceipt(string ApplicationId)
        {

            var paymentdetails = (from a in _context.PaymentLog where a.ApplicationId == ApplicationId select a).FirstOrDefault();

            if (paymentdetails != null)
            {
                string APIHash = paymentdetails.Rrreference + generalClass.AppKeyLive + generalClass.merchantIdLive; //generalClass.AppKey + generalClass.merchantId;
                string AppkeyHashed = generalClass.GenerateSHA512(APIHash);

                WebResponse webResponse = _utilityHelper.GetRemitaPaymentDetails(AppkeyHashed, paymentdetails.Rrreference);

                GetPaymentResponse paymentResponse = (GetPaymentResponse)webResponse.value;

                if (paymentResponse != null)
                {

                    if (paymentResponse.message == "Successful" || paymentResponse.status == "00")
                    {
                        paymentdetails.Status = "AUTH";
                        paymentdetails.TxnMessage = paymentResponse.message;
                        paymentdetails.TransactionId = paymentResponse.status;
                        paymentdetails.TransactionDate = Convert.ToDateTime(paymentResponse.transactiontime);
                        ResponseWrapper responseWrapper = _workflowHelper.processAction(ApplicationId, "GenerateRRR", _helpersController.getSessionEmail(), "Remita Retrieval Reference Generated");

                    }
                    else
                    {
                        paymentdetails.Status = "INIT";
                    }
                    _context.SaveChanges();
                }

                ViewBag.PaymentLogApplicationId = paymentResponse.orderId;
                ViewBag.PaymentLogStatus = paymentdetails.Status;
                ViewBag.PaymentLogRRReference = paymentResponse.RRR;
                ViewBag.PaymentLogTransactionDate = paymentResponse.transactiontime;
                ViewBag.PaymentLogTxnMessage = paymentResponse.message;
                ViewBag.TotalAmount = paymentdetails.TxnAmount;
            }

            return View();



        }



        public ActionResult DocumentUpload(string ApplicationId)
        {
            ViewBag.Rejectioncomments = _helpersController.AllHistoryComment(_helpersController.getSessionEmail());
            List<DocUpload> DocList = new List<DocUpload>();
            var linseofbusinessid = (from a in _context.ApplicationRequestForm where a.ApplicationId == ApplicationId select a.LineOfBusinessId).FirstOrDefault();
            var doclist = (from d in _context.Documents where d.LineOfBusinessId == linseofbusinessid select d).ToList();
            var uploadeddoc = (from d in _context.UploadedDocuments where d.ApplicationId == ApplicationId select d).ToList();
            ViewBag.MyApplicationID = ApplicationId;
            ViewBag.UploadedDocCount = uploadeddoc.Count;
            if (uploadeddoc.Count == 0 || doclist.Count() != uploadeddoc.Count())
            {
                if (doclist.Count > 0)
                {
                    foreach (var item in doclist)
                    {
                        DocList.Add(new DocUpload()
                        {

                            DocumentName = item.DocumentName,
                            DocId = item.DocId,
                            IsMandatory = item.IsMandatory

                        });
                    }
                }
            }
            else
            {
                foreach (var item in uploadeddoc)
                {
                    var docnamesplit = item.DocumentName.ToString().Split('_');
                    var uploadedfilenamesplit = item.DocumentSource.ToString().Split('_').Last();

                    DocList.Add(new DocUpload()
                    {
                        DocumentName = docnamesplit[0],
                        DocId = item.DocumentUploadId,
                        IsMandatory = "Y",
                        DocumentSource = item.DocumentSource,
                        UploadedDocName = uploadedfilenamesplit
                    });
                }
            }

            return View(DocList);
        }


        [HttpPost]
        public async Task<IActionResult> PostDocuments(IList<IFormFile> files, String AppID, int DocID, String DocName, String DocSource)//List<UploadedDocuments> MyApplicationDocs IList<IFormFile> files
        {


            var checkapprejected = (from a in _context.ApplicationRequestForm where a.ApplicationId == AppID select a.Status).FirstOrDefault();


            if (checkapprejected == "Rejected")
            {
                var deleteexistingdoc = (from u in _context.UploadedDocuments where u.DocumentUploadId == DocID select u).ToList();

                if (deleteexistingdoc.Count() > 0)
                {
                    foreach (var deleteitems in deleteexistingdoc)
                    {
                        _context.UploadedDocuments.Remove(deleteitems);
                    }
                    await _context.SaveChangesAsync();
                }

                if (files.Count > 0)
                {
                    foreach (var item in files)
                    {
                        string filename = ContentDispositionHeaderValue.Parse(item.ContentDisposition).FileName.Trim('"');

                        var fileName = Path.GetFileName(AppID + "_" + filename);
                        var filePath = Path.Combine(_hostingEnv.WebRootPath, "UploadedFiles", fileName);
                        if (System.IO.File.Exists(filePath))
                        {
                            // If file found, delete it 
                            System.IO.File.Delete(filePath);
                        }
                    }
                }

            }

            var check_doc1 = _context.UploadedDocuments.Where(x => x.ApplicationId == AppID && x.DocumentName == DocName).FirstOrDefault();


            if (files != null)
            {

                var check_doc = _context.UploadedDocuments.Where(x => x.ApplicationId == AppID && x.DocumentName == DocName).FirstOrDefault();

                if (check_doc1 == null || check_doc == null)
                {
                    UploadedDocuments submitDocs = new UploadedDocuments()
                    {
                        ApplicationId = AppID,
                        DocumentName = DocName,
                        DocumentSource = AppID + "_" + DocSource
                    };
                    _context.UploadedDocuments.Add(submitDocs);
                }

                else if (check_doc1 != null)
                {
                    check_doc.ApplicationId = AppID;
                    check_doc.DocumentName = DocName;
                    check_doc.DocumentSource = AppID + "_" + DocSource;
                }
                else
                {

                    _context.UploadedDocuments.Remove(check_doc1);
                    _context.SaveChanges();

                    UploadedDocuments submitDocs = new UploadedDocuments()
                    {
                        ApplicationId = AppID,
                        DocumentName = DocName,
                        DocumentSource = AppID + "_" + DocSource
                    };
                    _context.UploadedDocuments.Add(submitDocs);
                }
                _context.SaveChanges();
                foreach (var item in files)
                {
                    var fileName = Path.GetFileName(AppID + "_" + item.FileName);
                    var filePath = Path.Combine(_hostingEnv.WebRootPath, "UploadedFiles", fileName);
                    using (var fileSteam = new FileStream(filePath, FileMode.Create))
                    {
                        await item.CopyToAsync(fileSteam);
                    }
                }
            }

            return null;
        }



        [HttpPost]
        public JsonResult SubmitDocumentUpload(string applicationid)
        {

            string status = string.Empty;
            string message = string.Empty;
            string appid = string.Empty;

            ResponseWrapper responseWrapper = _workflowHelper.processAction(applicationid, "Submit", _helpersController.getSessionEmail(), "Application was successfully submitted after document upload");
            if (!responseWrapper.status)
            {
                status = "failure";
                return Json(status);
            }
            else
            {
                status = "success";
                return Json(status);

            }
        }





        [HttpGet]
        public ActionResult CompanyProfile()
        {

            var companydetails = (from a in _context.UserMaster where a.UserEmail == _helpersController.getSessionEmail() select a).FirstOrDefault();

            ViewBag.AllCompanyDocument = _helpersController.CompanyDocument(_helpersController.getSessionEmail());


            return View(companydetails);
        }



        public ActionResult UpdateCompanyRecord(UserMaster model)
        {
            string status = "success";
            string actionType = Request.Form["actionType"];
            string companyId = Request.Form["companyId"];
            var companydetails = (from u in _context.UserMaster where u.UserEmail == model.UserEmail select u).FirstOrDefault();
            if (actionType.Contains("UPDATE_PROFILE") && companydetails != null)
            {
                companydetails.CompanyName = model.CompanyName;
                companydetails.PhoneNum = model.PhoneNum;
            }
            else if (actionType.Contains("ADDRESS") && companydetails != null)
            {
                companydetails.CompanyAddress = model.CompanyAddress;
            }
            _context.SaveChanges();
            return Json(new
            {
                Status = status
            });
        }



        public JsonResult ByPassPayment(string applicationid, string PaymentName, decimal paymentamount)
        {

            decimal Arrears = 0;

            try
            {
                log.Info("ApplicationID =>" + applicationid);
                ApplicationRequestForm appRequest = _context.ApplicationRequestForm.Where(c => c.ApplicationId.Trim() == applicationid.Trim()).FirstOrDefault();
                PaymentLog paymentLog = _context.PaymentLog.Where(c => c.ApplicationId.Trim() == applicationid.Trim()).FirstOrDefault();
                paymentLog.ApplicationId = appRequest.ApplicationId;
                paymentLog.TransactionDate = DateTime.UtcNow;
                paymentLog.LastRetryDate = DateTime.UtcNow;
                paymentLog.PaymentCategory = PaymentName;
                paymentLog.TransactionId = "TXNID";
                paymentLog.ApplicantId = _helpersController.getSessionEmail();
                paymentLog.Rrreference = "RRR";
                paymentLog.AppReceiptId = "APPID";
                paymentLog.TxnAmount = paymentamount;
                paymentLog.Arrears = Arrears;
                paymentLog.Account = _context.Configuration.Where(c => c.ParamId == "AccountNumber").FirstOrDefault().ParamValue;
                paymentLog.BankCode = _context.Configuration.Where(c => c.ParamId == "BankCode").FirstOrDefault().ParamValue;
                paymentLog.RetryCount = 0;
                paymentLog.Status = "AUTH";
                _generalLogger.LogRequest($"{"ByPassPayment -- About to Add Payment Log"}{"-"}{DateTime.Now}", false, directory);
               
                _context.SaveChanges();
                _generalLogger.LogRequest($"{"ByPassPayment --Payment Log Saved it Successfully"}{"-"}{DateTime.Now}", false, directory);

                ResponseWrapper responseWrapper = _workflowHelper.processAction(applicationid, "GenerateRRR", _helpersController.getSessionEmail(), "Remita Retrieval Reference Generated");
                if (responseWrapper.status == true)
                {
                    return Json(new { Status = "success", Message = responseWrapper.value });

                }

            }
            catch (Exception ex)
            {
                _generalLogger.LogRequest($"{"ByPassPayment -- an exception occurred"} {ex}{"-"}{DateTime.Now}", true, directory);

            }

            return Json(new
            {
                Status = "success",
                Message = "Payment was successfully bypassed",
            });

        }


        [HttpPost]

        public JsonResult CertificateRenewal(string CertificateNumber)
        {
            var details = (from a in _context.ApplicationRequestForm where a.LicenseReference == CertificateNumber select a).FirstOrDefault();
            if (details != null)
            {
                if (details.AgencyId == 1)
                {
                    var appdetails = (from a in _context.ApplicationRequestForm
                                      join g in _context.GovernmentAgency on a.ApplicationId equals g.ApplicationId
                                      where a.LicenseReference == CertificateNumber
                                      select new
                                      {
                                          a.ApplicationId,
                                          a.AgencyId,
                                          a.LineOfBusinessId,
                                          DateofEstablishment = a.DateofEstablishment.ToString(),
                                          a.CompanyEmail,
                                          a.CompanyWebsite,
                                          a.AgencyName,
                                          a.PostalAddress,
                                          a.PhoneNum,
                                          a.CompanyAddress,
                                          a.CacregNum,
                                          a.NameOfAssociation,
                                          g.ServicesProvidedInPort,
                                          g.AnyOtherRelevantInfo,
                                          TxnAmount = (from a in _context.PaymentLog where a.ApplicationId == details.ApplicationId select a.TxnAmount).FirstOrDefault()
                                      }).ToList();

                    return Json(appdetails);
                }
                else if (details.AgencyId == 2)
                {
                    var appdetails = (from a in _context.ApplicationRequestForm
                                      join g in _context.LogisticsServiceProvider on a.ApplicationId equals g.ApplicationId
                                      where a.LicenseReference == CertificateNumber
                                      select new
                                      {
                                          a.ApplicationId,
                                          a.AgencyId,
                                          a.LineOfBusinessId,
                                          a.DateofEstablishment,
                                          a.CompanyEmail,
                                          a.CompanyWebsite,
                                          a.AgencyName,
                                          a.PostalAddress,
                                          a.CompanyAddress,
                                          g.AnyOtherInfo,
                                          g.CrffnRegistrationNum,
                                          g.CrffnRegistratonExpiryDate,
                                          g.CustomLicenseExpiryDate,
                                          g.CustomLicenseNum,
                                          g.LineOfBusiness,
                                          a.PhoneNum,
                                          g.OtherLicense,
                                          g.OtherLicenseExpiryDate,
                                          TxnAmount = (from a in _context.PaymentLog where a.ApplicationId == details.ApplicationId select a.TxnAmount).FirstOrDefault()

                                      }).ToList();
                    return Json(appdetails);
                }
                else if (details.AgencyId == 3)
                {
                    var appdetails = (from a in _context.ApplicationRequestForm
                                      where a.LicenseReference == CertificateNumber
                                      select new
                                      {
                                          a.ApplicationId,
                                          a.AgencyId,
                                          a.LineOfBusinessId,
                                          a.DateofEstablishment,
                                          a.CompanyEmail,
                                          a.CompanyWebsite,
                                          a.AgencyName,
                                          a.PostalAddress,
                                          a.CompanyAddress,
                                          a.PhoneNum,
                                          Terminal = (from t in _context.PortOffDockTerminalOperator where t.ApplicationId == details.ApplicationId select t).ToList(),
                                          TxnAmount = (from c in _context.PaymentLog where a.ApplicationId == details.ApplicationId select c.TxnAmount).FirstOrDefault()
                                      }).ToList();
                    return Json(appdetails);
                }
                else if (details.AgencyId == 4)
                {
                    var appdetails = (from a in _context.ApplicationRequestForm
                                      join g in _context.ShippingAgency on a.ApplicationId equals g.ApplicationId
                                      join p in _context.PaymentLog on a.ApplicationId equals p.ApplicationId
                                      where a.LicenseReference == CertificateNumber
                                      select new
                                      {
                                          ApplicationId = a.ApplicationId,
                                          AgencyId = a.AgencyId,
                                          LineOfBusinessId = a.LineOfBusinessId,
                                          DateofEstablishment = a.DateofEstablishment,
                                          CompanyEmail = a.CompanyEmail,
                                          CompanyWebsite = a.CompanyWebsite,
                                          AgencyName = a.AgencyName,
                                          PostalAddress = a.PostalAddress,
                                          PhoneNum = a.PhoneNum,
                                          CompanyAddress = a.CompanyAddress,
                                          AnyOtherInfo = g.AnyOtherInfo,
                                          CargoType = g.CargoType,
                                          LineOfBusiness = g.LineOfBusiness,
                                          VesselLinesRepresentedInNigeria = g.VesselLinesRepresentedInNigeria,
                                          TxnAmount = p.TxnAmount//(from a in _context.PaymentLog where a.ApplicationId == details.ApplicationId select a.TxnAmount).FirstOrDefault()

                                      }).ToList();
                    return Json(appdetails);
                }
                else if (details.AgencyId == 5)
                {
                    var appdetails = (from a in _context.ApplicationRequestForm
                                      join g in _context.OtherPortServiceProvider on a.ApplicationId equals g.ApplicationId
                                      where a.LicenseReference == CertificateNumber
                                      select new
                                      {
                                          a.ApplicationId,
                                          a.AgencyId,
                                          a.LineOfBusinessId,
                                          a.DateofEstablishment,
                                          a.CompanyEmail,
                                          a.CompanyWebsite,
                                          a.AgencyName,
                                          a.PostalAddress,
                                          a.PhoneNum,
                                          a.CompanyAddress,
                                          g.AnyOtherInfo,
                                          g.LineOfBusiness,
                                          TxnAmount = (from a in _context.PaymentLog where a.ApplicationId == details.ApplicationId select a.TxnAmount).FirstOrDefault()

                                      }).ToList();
                    return Json(appdetails);
                }

                else if (details.AgencyId == 6)
                {
                    var appdetails = (from a in _context.ApplicationRequestForm
                                      join g in _context.UserOfPortService on a.ApplicationId equals g.ApplicationId
                                      where a.LicenseReference == CertificateNumber
                                      select new
                                      {
                                          a.ApplicationId,
                                          a.AgencyId,
                                          a.LineOfBusinessId,
                                          a.DateofEstablishment,
                                          a.CompanyEmail,
                                          a.CompanyWebsite,
                                          a.AgencyName,
                                          a.PostalAddress,
                                          a.PhoneNum,
                                          a.CompanyAddress,
                                          g.NepcRegNo,
                                          g.AnyOtherInfo,
                                          g.Category,
                                          TxnAmount = (from a in _context.PaymentLog where a.ApplicationId == details.ApplicationId select a.TxnAmount).FirstOrDefault()

                                      }).ToList();
                    return Json(appdetails);
                }
            }
            return Json("");
        }



        [HttpPost]
        public JsonResult GetCertificateNumber()
        {
            var certificatelist = (from a in _context.ApplicationRequestForm where a.LicenseReference != null && a.CompanyEmail == _helpersController.getSessionEmail() select new { a.LicenseReference }).ToList();
            return Json(certificatelist);
        }





        public JsonResult GetAllCategory()
        {
            var Agencydetails = (from a in _context.LineOfBusiness orderby a.LineOfBusinessName ascending select new { a.LineOfBusinessId, a.LineOfBusinessName }).ToList();
            return Json(Agencydetails);
        }



        public JsonResult AllAgencyFees()
        {
            var Feedetails = (from a in _context.PaymentCategory select new { PaymentAmount = Convert.ToDecimal(a.PaymentAmount).ToString("N"), a.PaymentCategoryName }).ToList();
            return Json(Feedetails);
        }





        [HttpGet]
        public ActionResult MyDocuments()
        {


            ViewBag.AllCompanyDocument = _helpersController.CompanyDocument(_helpersController.getSessionEmail());


            return View();
        }




        public ActionResult ALLCompanyPermits()
        {
            return View();
        }





        [HttpPost]
        public ActionResult GetAllCertificate()
        {
            var companyemail = _helpersController.getSessionEmail();
            var draw = Request.Form["draw"].FirstOrDefault();
            var start = Request.Form["start"].FirstOrDefault();
            var length = Request.Form["length"].FirstOrDefault();

            var sortColumn = Request.Form["columns[" + Request.Form["order[0][column]"].FirstOrDefault() + "][name]"].FirstOrDefault();
            var sortColumnDir = Request.Form["order[0][dir]"].FirstOrDefault();

            var searchTxt = Request.Form["search[value]"][0];

            int pageSize = length != null ? Convert.ToInt32(length) : 0;
            int skip = start != null ? Convert.ToInt32(start) : 0;
            int totalRecords = 0;
            var today = DateTime.Now.Date;

            var staff = (from p in _context.ApplicationRequestForm
                         where (p.LicenseReference != null && p.CompanyEmail == companyemail && p.IsLegacy == "NO")
                         select new
                         {
                             applicationId = p.ApplicationId,
                             licenseReference = p.LicenseReference,
                             companyEmail = p.CompanyEmail,
                             companyAddress = p.CompanyAddress,
                             applicationTypeId = p.ApplicationTypeId,
                             agencyName = p.AgencyName
                         });


            if (!(string.IsNullOrEmpty(sortColumn) && string.IsNullOrEmpty(sortColumnDir)))
            {
                staff = staff.OrderBy(s => s.applicationId + " " + sortColumnDir);
            }
            if (!string.IsNullOrEmpty(searchTxt))
            {
                staff = staff.Where(a => a.applicationId.Contains(searchTxt) || a.agencyName.Contains(searchTxt) || a.licenseReference.Contains(searchTxt)
               || a.companyEmail.Contains(searchTxt) || a.applicationTypeId.Contains(searchTxt) || a.companyAddress.Contains(searchTxt));
            }
            totalRecords = staff.Count();
            var data = staff.Skip(skip).Take(pageSize).ToList();
            return Json(new { draw = draw, recordsFiltered = totalRecords, recordsTotal = totalRecords, data = data });

        }






        [HttpGet]
        public ActionResult ChangePassword()
        {

            return View();
        }

        [HttpPost]
        public ActionResult ChangePassword(PasswordModel model)
        {
            string responseMessage = null;

            try
            {
                AppResponse appResponse = _helpersController.ChangePassword(_helpersController.getSessionEmail(), model.OldPassword, model.NewPassword);
                _generalLogger.LogRequest($"{"Response from Elps =>" + appResponse.message}{"-"}{DateTime.Now}", false, directory);

                if (appResponse.message.Trim() != "SUCCESS")
                {
                    _generalLogger.LogRequest($"{"Response from Elps, An Error Message occured during Service Call to Elps Server, Please try again Later =>" + appResponse.message}{"-"}{DateTime.Now}", false, directory);

                    responseMessage = "An Error Message occured during Service Call to Elps Server, Please try again Later";
                }
                else
                {
                    if (((bool)appResponse.value) == true)
                    {
                        {
                            _generalLogger.LogRequest($"{"password was successfully changed"}{"-"}{DateTime.Now}", false, directory);
                            responseMessage = "success";
                            TempData["success"] = _helpersController.getSessionEmail() + " password was successfully changed";
                        }
                    }
                    else
                    {
                        responseMessage = "Password Cannot Change, Kindly ensure your Old Password is correct and try again";
                    }
                }
            }
            catch (Exception ex)
            {
                _generalLogger.LogRequest($"{"A General Error occured during Change Password"}{"-"}{DateTime.Now}", false, directory);

                responseMessage = "A General Error occured during Change Password";
            }

            return Json(new
            {
                Message = responseMessage
            }
             );
        }





        [HttpGet]
        public ActionResult MyPayments()
        {
            List<PaymentLog> PaymentLogList = new List<PaymentLog>();
            try
            {
                //var dbCtxt = DbManager.getConnectionEntities();
                PaymentLogList = _context.PaymentLog.Where(p => p.ApplicantId == _helpersController.getSessionEmail()).ToList();
                ViewBag.MyPaymentsResponseMessage = "SUCCESS";
            }
            catch (Exception ex)
            {
                _generalLogger.LogRequest($"{"Error Occured Getting Payment List"} {ex}{"-"}{DateTime.Now}", false, directory);

                ViewBag.MyPaymentsResponseMessage = "Error Occured Getting Payment List, Please Try Again Later";
            }

            return View(PaymentLogList);
        }





        [HttpGet]
        public ActionResult RouteApplication(string ApplicationId = null)
        {
            ApplicationRequestForm appRequest = _context.ApplicationRequestForm.Where(c => c.ApplicationId.Trim() == ApplicationId.Trim()).FirstOrDefault();
            string actionName = "";

            switch (appRequest.CurrentStageId)
            {
                case 1:
                    actionName = "ApplicationForm";
                    break;
                case 2:
                    actionName = "ChargeSummary";
                    break;
                case 3:
                    actionName = "DocumentUpload";
                    break;

            }

            return RedirectToAction(actionName, new
            {
                ApplicationId = appRequest.ApplicationId
            });



        }



        public JsonResult GetFees(int MyCategoryid)
        {

            var details = _utilityHelper.Fees(MyCategoryid);

            return Json(new { feeamount = details.Amount, formid = details.FormTypeId });
        }


        public async Task<ActionResult> GenerateRRR(string ApplicationId, string Amount)
        {
            string resultrrr = "";
            decimal amt = 0;
            var checkRRRExit = (from a in _context.PaymentLog where a.ApplicationId == ApplicationId select a).FirstOrDefault();
            var checkGovAgency = (from a in _context.ApplicationRequestForm join l in _context.LineOfBusiness
                                  on a.LineOfBusinessId equals l.LineOfBusinessId
                                  where a.ApplicationId == ApplicationId select new { a, l }).FirstOrDefault();
            if (checkGovAgency?.a.AgencyId == 1)
            {
                checkGovAgency.a.CurrentStageId = 3;
                _context.SaveChanges();
                return RedirectToAction("DocumentUpload", new { ApplicationId = ApplicationId });
            }
            if (checkRRRExit == null)
            {
                amt = Convert.ToDecimal(Amount);
                var baseUrl = HttpContext.Request.Scheme + "://" + HttpContext.Request.Host + "" + "" + HttpContext.Request.PathBase;
                resultrrr = await _utilityHelper.GeneratePaymentReference(ApplicationId, baseUrl, checkGovAgency.a.AgencyName, checkGovAgency.l.Amount);
            }
            else
            {
                amt = Convert.ToDecimal(checkRRRExit.TxnAmount);
                resultrrr = checkRRRExit.Rrreference;
            }
            return RedirectToAction("ChargeSummary", new { RRR = resultrrr, applicationId = ApplicationId, amount = amt });
        }




        public ActionResult ViewCertificate(string id)
        {
            var Host = HttpContext.Request.Scheme + "://" + HttpContext.Request.Host + "" + "" + HttpContext.Request.PathBase;

            return _helpersController.ViewCertificate(id, Host);
        }



    }
}
