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
using System.Net.Http.Headers;
using Microsoft.Extensions.Logging;
using Rotativa.AspNetCore;
using RSPP.Models.DTOs;
using Microsoft.Extensions.Options;
using RSPP.Models.Options;
using RSPP.Services.Interfaces;
using RSPP.UnitOfWorks.Interfaces;
using RSPP.Models.DTOs.Remita;
using Newtonsoft.Json;
using RSPP.Models.ViewModels;

namespace RSPP.Controllers
{
    public class CompanyController : AppUserController
    {
        public RSPPdbContext _context;
        public IConfiguration _configuration;
        WorkFlowHelper _workflowHelper;
        IHttpContextAccessor _httpContextAccessor;
        GeneralClass generalClass = new GeneralClass();
        ResponseWrapper responseWrapper = new ResponseWrapper();
        List<ApplicationRequestForm> AppRequest = null;
        HelperController _helpersController;
        UtilityHelper _utilityHelper;

        private ILog log = log4net.LogManager.GetLogger(typeof(CompanyController));
        protected readonly ILogger<CompanyController> _logger;

        private readonly IWebHostEnvironment _hostingEnv;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IPaymentService _paymentService;
        private readonly IRemitaPaymentService _remitaPaymentService;
        private readonly RemitaOptions _remitaOptions;

        private const string FAILED_UPDATE_RESPONSE = "Update Failed";
        private const string SUCCESS_UPDATE_RESPONSE = "Update Successful";


        [Obsolete]
        public CompanyController(
            RSPPdbContext context,
            IConfiguration configuration,
            IHttpContextAccessor httpContextAccessor,
            IWebHostEnvironment hostingEnv,
            ILogger<CompanyController> logger,
            IPaymentService paymentService,
            IRemitaPaymentService remitaPaymentService,
            IOptions<RemitaOptions> remitaOptions,
            IUnitOfWork unitOfWork
            ) : base(hostingEnv, paymentService)
        {
            _context = context;
            _configuration = configuration;
            _httpContextAccessor = httpContextAccessor;
            _hostingEnv = hostingEnv;
            _helpersController = new HelperController(_context, _configuration, _httpContextAccessor);
            _workflowHelper = new WorkFlowHelper(_context);
            _utilityHelper = new UtilityHelper(_context);
            _logger = logger;
            _remitaPaymentService = remitaPaymentService;
            _unitOfWork = unitOfWork;
            _remitaOptions = remitaOptions.Value;
        }


        /// <summary>
        /// Fetches various metrics of the company
        /// - all application status
        /// - messages
        /// - user guides
        /// </summary>
        /// <returns>A view result</returns>

        public IActionResult Index()
        {
            string responseMessage = null;
            Dictionary<string, int> appStatistics;
            Dictionary<string, int> appStageReference = null;
            var companyemail = _helpersController.getSessionEmail();
            try
            {
                log.Info("About To Generate User DashBoard Information");
                ViewBag.TotalPermitCount = 0;
                ViewBag.ApplicationCount = 0;
                ViewBag.PermitExpiringCount = 0;
                ViewBag.ProcessedApplicationCount = 0;
                ViewBag.CompanyName = _helpersController.getSessionCompanyName();

                //get action history message & action id for the current user
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
                // fetch all applications ids & currentstage for current user
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

                log.Info("About To Get Applications and Company Notification Messages");
                var userMaster = (from u in _context.UserMaster where u.UserEmail == companyemail select u).FirstOrDefault();
                if (userMaster is null)
                {
                    // implement later
                }

                ViewBag.UserGuides = this.GetUserGuides(USER_TYPE_COMPANY, USER_GUIDES_COMPANY_PATH);
                ViewBag.VideoGuide = this.VideoUserGuide();

                ViewBag.AllMessages = _helpersController.GetCompanyMessages(_context, userMaster);

                // fetch all applications with licenses
                ViewBag.AllUserApplication = _helpersController.GetApplications(userMaster.UserEmail, "ALL", out responseMessage);

                log.Info("GetApplications ResponseMessage => " + responseMessage);

                if (responseMessage == "SUCCESS")
                {
                    // get messages from unapproved applilcations
                    ViewBag.Allcomments = _helpersController.AllHistoryComment(_helpersController.getSessionEmail());

                    appStatistics = _helpersController.GetApplicationStatistics(_helpersController.getSessionEmail(), out responseMessage, out appStageReference);
                    ViewBag.TotalPermitCount = appStatistics["PEM"];
                    ViewBag.ApplicationCount = appStatistics["ALL"];
                    ViewBag.PermitExpiringCount = appStatistics["EXP"];
                    ViewBag.ProcessedApplicationCount = appStatistics["PROC"];

                    //ViewBag.AllUserApplication = _helpersController.GetApplications(_helpersController.getSessionEmail(), "ALL", out responseMessage);
                    //ViewBag.AllUserApplication = _helpersController.GetUnissuedLicense(_helpersController.getSessionEmail(), "ALL", out responseMessage);

                    log.Info("GetApplicationStatistics ResponseMessage=> " + responseMessage);
                }

                log.Info("GetApplicationStatistics Count => " + appStageReference.Count);
                ViewBag.StageReferenceList = appStageReference;
                ViewBag.ErrorMessage = responseMessage;


            }
            catch (Exception ex)
            {
                log.Error(ex.StackTrace + Environment.NewLine + "InnerException =>" + ex.InnerException);
                ViewBag.ErrorMessage = "Error Occured Getting the Company DashBoard, Please Try again Later";
            }

            return View();
        }

        /// <summary>
        /// Updates application notifications whose messages have been read by the company
        /// </summary>
        /// <param name="applicationId">the application id to be updated</param>
        /// <returns>A BasicResponse indicating success or failure of this operation</returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public JsonResult UpdateReadApplications(string applicationId)
        {
            var response = new BasicResponse(false, FAILED_UPDATE_RESPONSE);
            try
            {

                // provide better applicationId validation later
                if (!string.IsNullOrWhiteSpace(applicationId))
                {
                    var companyemail = _helpersController.getSessionEmail();
                    var selectedApplication = _context.ApplicationRequestForm
                        .FirstOrDefault(app => app.ApplicationId == applicationId && app.CompanyEmail == companyemail);
                    if (selectedApplication != null)
                    {
                        selectedApplication.IsRead = true;
                        _context.ApplicationRequestForm.Update(selectedApplication);
                        _context.SaveChanges();

                        response.Success = true;
                        response.ResultMessage = SUCCESS_UPDATE_RESPONSE;
                    }
                }

                return Json(response);
            }
            catch (Exception ex)
            {
                response.ResultMessage += " - " + ex.Message;
                return Json(response);
                //throw;
            }

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
            //ApplicationRequestForm appdetails = null;
            //GovernmentAgency govagencydetails = null;
            //LogisticsServiceProvider logisticsserviceprovider = null;
            //OtherPortServiceProvider otherportserviceprovider = null;
            //PortOffDockTerminalOperator portoffdockserviceprovider = null;
            //ShippingAgency shippingagency = null;
            //UserOfPortService userofportservice = null;
            //var generatedapplicationid = generalClass.GenerateApplicationNo();
            //var checkappexist = (from a in _context.ApplicationRequestForm where a.ApplicationId == model.ApplicationId select a).FirstOrDefault();
            //var companyemail = _helpersController.getSessionEmail();
            //var appid = checkappexist == null ? generatedapplicationid : model.ApplicationId;
            //try
            //{
            //    appdetails = checkappexist == null ? new ApplicationRequestForm() : (from a in _context.ApplicationRequestForm where a.ApplicationId == model.ApplicationId select a).FirstOrDefault();
            //    govagencydetails = (from a in _context.GovernmentAgency where a.ApplicationId == model.ApplicationId select a).FirstOrDefault() == null ? new GovernmentAgency() : (from a in _context.GovernmentAgency where a.ApplicationId == model.ApplicationId select a).FirstOrDefault();
            //    logisticsserviceprovider = (from a in _context.LogisticsServiceProvider where a.ApplicationId == model.ApplicationId select a).FirstOrDefault() == null ? new LogisticsServiceProvider() : (from a in _context.LogisticsServiceProvider where a.ApplicationId == model.ApplicationId select a).FirstOrDefault();
            //    otherportserviceprovider = (from a in _context.OtherPortServiceProvider where a.ApplicationId == model.ApplicationId select a).FirstOrDefault() == null ? new OtherPortServiceProvider() : (from a in _context.OtherPortServiceProvider where a.ApplicationId == model.ApplicationId select a).FirstOrDefault();
            //    portoffdockserviceprovider = (from a in _context.PortOffDockTerminalOperator where a.ApplicationId == model.ApplicationId select a).FirstOrDefault() == null ? new PortOffDockTerminalOperator() : (from a in _context.PortOffDockTerminalOperator where a.ApplicationId == model.ApplicationId select a).FirstOrDefault();
            //    shippingagency = (from a in _context.ShippingAgency where a.ApplicationId == model.ApplicationId select a).FirstOrDefault() == null ? new ShippingAgency() : (from a in _context.ShippingAgency where a.ApplicationId == model.ApplicationId select a).FirstOrDefault();
            //    userofportservice = (from a in _context.UserOfPortService where a.ApplicationId == model.ApplicationId select a).FirstOrDefault() == null ? new UserOfPortService() : (from a in _context.UserOfPortService where a.ApplicationId == model.ApplicationId select a).FirstOrDefault();

            //    appdetails.ApplicationTypeId = model.ApplicationTypeId;
            //    appdetails.Status = appdetails.Status == null ? "ACTIVE" : "Rejected";
            //    appdetails.ApplicationId = appid;
            //    appdetails.AgencyName = model.AgencyName;
            //    appdetails.DateofEstablishment = Convert.ToDateTime(model.DateofEstablishment);
            //    appdetails.CompanyAddress = model.CompanyAddress;
            //    appdetails.PostalAddress = model.PostalAddress;
            //    appdetails.PhoneNum = model.PhoneNum;
            //    appdetails.AddedDate = DateTime.Now;
            //    appdetails.ModifiedDate = DateTime.Now;
            //    appdetails.LastAssignedUser = companyemail;
            //    appdetails.CompanyEmail = companyemail;
            //    appdetails.CompanyWebsite = model.CompanyWebsite;
            //    appdetails.AgencyId = model.AgencyId;
            //    appdetails.CurrentStageId = 1;
            //    appdetails.PrintedStatus = "Not Printed";
            //    appdetails.NameOfAssociation = model.NameOfAssociation;
            //    appdetails.CacregNum = model.CacregNum;
            //    appdetails.LineOfBusinessId = Convert.ToInt32(Request.Form["lineofbusinessid"]);
            //    if (checkappexist == null)
            //    {
            //        _context.Add(appdetails);
            //    }

            //    if (model.AgencyId != 0)
            //    {

            //        if (model.AgencyId == 1)//government Agency
            //        {
            //            govagencydetails.ApplicationId = appid;
            //            govagencydetails.ServicesProvidedInPort = Request.Form["ServicesProvidedInPort"].ToString();
            //            govagencydetails.AnyOtherRelevantInfo = Request.Form["AnyOtherRelevantInfo"].ToString();

            //            if (model.ApplicationId == null)
            //            {
            //                _context.Add(govagencydetails);
            //            }
            //        }
            //        else if (model.AgencyId == 2)//Logistics Services Providers
            //        {
            //            logisticsserviceprovider.ApplicationId = appid;
            //            logisticsserviceprovider.LineOfBusiness = Request.Form["LogisticsLineOfBusiness"].ToString();
            //            logisticsserviceprovider.CustomLicenseNum = Request.Form["CustomLicenseNum"].ToString();
            //            logisticsserviceprovider.CrffnRegistrationNum = Request.Form["CrffnRegistrationNum"].ToString();
            //            logisticsserviceprovider.AnyOtherInfo = Request.Form["LogisticsAnyOtherRelevantInfo"].ToString();
            //            logisticsserviceprovider.OtherLicense = model.OtherLicense == null || model.OtherLicense == "" ? model.OtherLicense : Request.Form["OtherLicense"].ToString();
            //            logisticsserviceprovider.CustomLicenseExpiryDate = DateTime.ParseExact(Request.Form["CustomLicenseExpiryDate"], "MM/dd/yyyy", CultureInfo.InvariantCulture);
            //            logisticsserviceprovider.CrffnRegistratonExpiryDate = DateTime.ParseExact(Request.Form["CrffnRegistratonExpiryDate"], "MM/dd/yyyy", CultureInfo.InvariantCulture);
            //            logisticsserviceprovider.OtherLicenseExpiryDate = model.OtherLicenseExpiryDate == null ? model.OtherLicenseExpiryDate : DateTime.ParseExact(Request.Form["OtherLicenseExpiryDate"], "MM/dd/yyyy", CultureInfo.InvariantCulture);
            //            if (model.ApplicationId == null)
            //            {
            //                _context.Add(logisticsserviceprovider);
            //            }
            //        }

            //        else if (model.AgencyId == 3)//Port/Off Dock Terminals Operators
            //        {

            //            if (MyTerminals.Count > 0)
            //            {


            //                foreach (var item in MyTerminals)
            //                {
            //                    var checkterminalexist = (from t in _context.PortOffDockTerminalOperator where t.ApplicationId == model.ApplicationId && t.LocationOfTerminal == item.TerminalLocation select t).ToList();

            //                    if (checkterminalexist.Count == 0)
            //                    {

            //                        PortOffDockTerminalOperator terminals = new PortOffDockTerminalOperator()
            //                        {
            //                            ApplicationId = appid,
            //                            NameOfTerminal = item.TerminalName,
            //                            LocationOfTerminal = item.TerminalLocation,
            //                            LineOfBusiness = Request.Form["portoffdockLineOfBusiness"].ToString(),
            //                            StatusOfTerminal = Request.Form["StatusOfTerminal"].ToString(),
            //                            CargoType = Request.Form["portoffdockCargoType"].ToString(),
            //                            AnyOtherInfo = Request.Form["portoffdockAnyOtherInfo"].ToString()
            //                        };
            //                        _context.PortOffDockTerminalOperator.Add(terminals);
            //                        _context.SaveChanges();
            //                    }

            //                    else if (checkterminalexist.Count > 0 && MyTerminals.Count == checkterminalexist.Count)
            //                    {

            //                        checkterminalexist.FirstOrDefault().NameOfTerminal = item.TerminalName;
            //                        checkterminalexist.FirstOrDefault().LocationOfTerminal = item.TerminalLocation;
            //                    }


            //                }
            //            }

            //            //if (model.ApplicationId == null)
            //            //{
            //            //    _context.Add(portoffdockserviceprovider);
            //            //}
            //        }

            //        else if (model.AgencyId == 4)//Shipping Agencies/Companies/Lines
            //        {
            //            shippingagency.ApplicationId = appid;
            //            shippingagency.LineOfBusiness = Request.Form["ShippingagencyLineOfBusiness"].ToString();
            //            shippingagency.VesselLinesRepresentedInNigeria = Request.Form["VesselLinesRepresentedInNigeria"].ToString();
            //            shippingagency.CargoType = Request.Form["ShippingagencyCargoType"].ToString();
            //            shippingagency.AnyOtherInfo = Request.Form["ShippingagencyAnyOtherInfo"].ToString();
            //            if (model.ApplicationId == null)
            //            {
            //                _context.Add(shippingagency);
            //            }
            //        }

            //        else if (model.AgencyId == 5) //Other Port Service Providers
            //        {
            //            otherportserviceprovider.ApplicationId = appid;
            //            otherportserviceprovider.LineOfBusiness = Request.Form["otherportLineOfBusiness"].ToString();
            //            otherportserviceprovider.AnyOtherInfo = Request.Form["otherportAnyOtherInfo"].ToString();

            //            if (model.ApplicationId == null)
            //            {
            //                _context.Add(otherportserviceprovider);
            //            }
            //        }

            //        else if (model.AgencyId == 6)// Users of Port
            //        {
            //            userofportservice.ApplicationId = appid;
            //            userofportservice.Category = Request.Form["userportLineOfBusiness"].ToString();
            //            userofportservice.AnyOtherInfo = Request.Form["userportAnyOtherInfo"].ToString();
            //            userofportservice.NepcRegNo = Request.Form["nepcregnum"].ToString();
            //            if (model.ApplicationId == null)
            //            {
            //                _context.Add(userofportservice);
            //            }
            //        }

            //    }

            //}
            //catch (Exception ex)
            //{
            //    message = "Unable to save record " + ex.Message;
            //    status = "failed";
            //    return Json(new { Status = status, applicationId = appid, Message = message });

            //}


            //_context.SaveChanges();

            // prepare & save details based on the type of service provider - returns success, rejected etc
            status = _helpersController.ApplicationForm(model, MyTerminals, "NO", appid);

            if (status == "Rejected")
            {
                responseWrapper = _workflowHelper.processAction(appid, "GenerateRRR", companyemail, "Payment recieved");
                if (responseWrapper.status == true)
                {
                    return Json(new { Status = "resubmit", applicationId = appid, Message = responseWrapper.value });

                }
                return Json(new { Status = status, applicationId = appid, Message = "Unable to resubmit your application please try again later." });
            }
            // process an application by moving it to its correct work stage
            responseWrapper = _workflowHelper.processAction(appid, "Proceed", companyemail, "Initiated Application");
            if (responseWrapper.status == true)
            {
                return Json(new { Status = "success", applicationId = appid, Message = responseWrapper.value });

            }

            return Json(new { Status = status, applicationId = appid, Message = "Unable to submit your application please try again later." });
            //message = "Your record was saved successfully";
            //status = "success";
            //if (checkappexist != null)
            //{
            //    if (checkappexist.Status == "Rejected")// if re-submission jump payment page and proceed to document upload page
            //    {
            //        appdetails.CurrentStageId = 2;
            //        _context.SaveChanges();
            //        responseWrapper = _workflowHelper.processAction(appid, "GenerateRRR", companyemail, "Payment recieved");
            //        if (responseWrapper.status == true)
            //        {
            //            return Json(new { Status = "resubmit", applicationId = appid, Message = responseWrapper.value });

            //        }

            //        return Json(new { Status = status, applicationId = appid, Message = message });
            //    }
            //}
            //responseWrapper = _workflowHelper.processAction(appid, "Proceed", companyemail, "Initiated Application");
            //if (responseWrapper.status == true)
            //{
            //    return Json(new { Status = "success", applicationId = appid, Message = responseWrapper.value });

            //}

            //return Json(new { Status = status, applicationId = appid, Message = message });
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
                log.Error(ex.StackTrace);
                ViewBag.ResponseMessage = "Error Occured Getting  Application List, Please Try Again Later";
            }
            return View(apps);
        }



        [HttpGet]
        public ActionResult MyLegacyApplications()
        {
            List<ApplicationRequestForm> legacyapp = new List<ApplicationRequestForm>();
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
                                 CurrentStageId = p.CurrentStageId
                             }).ToList();

            }
            catch (Exception ex)
            {
                log.Error(ex.StackTrace);
                ViewBag.ResponseMessage = "Error Occured Getting  Application List, Please Try Again Later";
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

                message = "success";
                _context.SaveChanges();
            }
            catch (Exception ex) { message = ex.Message; }

            return Json(message);
        }

        /// <summary>
        /// Fetches the details of an application's payment
        /// </summary>
        /// <param name="applicationId">the application id to be updated</param>
        /// <returns>A view to display the details</returns>

        [HttpGet]
        public ActionResult ChargeSummary(string applicationId)
        {
            var model = new ChargeSummaryVM();
            var chargeSummaryDetails = _unitOfWork.PaymentLogRepository.GetChargeSummary(applicationId);
            if (chargeSummaryDetails != null)
            {
                model = chargeSummaryDetails;
                model.MerchantId = _remitaOptions.MerchantId;
                model.BaseUrl = _remitaOptions.PortalBaseUrl;
                model.FinalizePaymentURL = _remitaOptions.FinalizePaymentURL;
                model.ApiKey = Encryption.GenerateSHA512($"{_remitaOptions.MerchantId}{model.RRR}{_remitaOptions.ApiKey}");
                return View(model);
            }
            //return RedirectToAction("GenerateRRR", new { applicationId });
            model.ErrorMessage = $"{AppMessages.PAYMENT} {AppMessages.NOT_EXIST}";
            return View(model);

        }

        /// <summary>
        /// Displays a receipt after a user makes a payment
        /// </summary>
        /// <param name="applicationId">application id</param>
        /// <returns>A view</returns>
        [HttpGet]
        public ActionResult PaymentReceipt(string applicationId)
        {
            var model = new PaymentReceiptVM();
            var transactionResponse = JsonConvert
                .DeserializeObject<RemitaTransactionResponse>(_paymentService.CheckPaymentStatusAsync(applicationId).ToString());

            if (transactionResponse != null)
            {
                var paymentDetails = _unitOfWork.PaymentLogRepository
                    .Get(pay => pay.ApplicationId == applicationId, null, "", null, null)
                    .FirstOrDefault();

                model.ApplicationId = applicationId;
                model.PaymentStatus = paymentDetails.Status;
                model.RRR = paymentDetails.Rrreference;
                model.TransactionDate = paymentDetails.TransactionDate.ToString();
                model.TransactionMessage = paymentDetails.TxnMessage;
                model.TotalAmount = paymentDetails.TxnAmount.ToString();
            }

            return View(model);
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
            if (linseofbusinessid != 1)
            {
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
            else
            {
                SubmitDocumentUpload(ApplicationId);
                return RedirectToAction("MyApplications");
            }


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

                log.Info("About to Add Payment Log");
                //_context.PaymentLog.Add(paymentLog);

                log.Info("Added Payment Log to Table");
                _context.SaveChanges();
                log.Info("Saved it Successfully");

                ResponseWrapper responseWrapper = _workflowHelper.processAction(applicationid, "GenerateRRR", _helpersController.getSessionEmail(), "Remita Retrieval Reference Generated");
                if (responseWrapper.status == true)
                {
                    return Json(new { Status = "success", Message = responseWrapper.value });

                }

            }
            catch (Exception ex)
            {
                log.Error(ex.StackTrace, ex);
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
                log.Info("Response from Shippers council =>" + appResponse.message);
                if (appResponse.message.Trim() != "SUCCESS")
                {
                    responseMessage = "An Error occured, Please try again Later";
                }
                else
                {
                    if (((bool)appResponse.value) == true)
                    {
                        {
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
                log.Error(ex.StackTrace);
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
                log.Error(ex.StackTrace);
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

        public ActionResult GenerateRRR(string applicationId, string amount)
        {
            string rrr = string.Empty;
            var applicationDetails = _unitOfWork.ApplicationRequestFormRepository
                .Get(app => app.ApplicationId == applicationId, null, "", null, null).FirstOrDefault();

            // gov agencies don't make payment, so move them to the next stage
            if (applicationDetails.AgencyId == 1)
            {
                applicationDetails.CurrentStageId = 3;
                _unitOfWork.Complete();
                return RedirectToAction("DocumentUpload", new { ApplicationId = applicationId });
            }

            var existingApplicationPayments = _unitOfWork.PaymentLogRepository
                .Get(app => app.ApplicationId == applicationId, null, "", null, null).FirstOrDefault();
            if (existingApplicationPayments == null)
            {

                var companyDetails = _unitOfWork.UserMasterRepository
                .Get(user => user.UserEmail == applicationDetails.CompanyEmail, null, "", null, null).FirstOrDefault();
                if (companyDetails != null)
                {

                    var requestRRR = new RRRRequestModel
                    {
                        serviceTypeId = applicationDetails.ApplicationTypeId == AppMessages.NEW ? _remitaOptions.NewServiceId : _remitaOptions.RenewalServiceId,
                        orderId = applicationId,
                        amount = amount,
                        payerName = companyDetails.CompanyName,
                        payerEmail = companyDetails.UserEmail,
                        payerPhone = companyDetails.PhoneNum,
                        description = applicationDetails.AgencyName
                    };
                    var initiatePaymentResponse = _remitaPaymentService.InitiatePaymentAsync(requestRRR).Result;
                    if (initiatePaymentResponse != null)
                    {
                        if (initiatePaymentResponse.Success)
                        {
                            InsertPaymentLog(initiatePaymentResponse, requestRRR);
                        }
                    }
                }

            }
            return RedirectToAction("ChargeSummary", new { applicationId });
        }

        /// <summary>
        /// adds payment details
        /// </summary>
        /// <param name="status">status string</param>
        /// <returns>A tuple consisting of success & message</returns>
        private void InsertPaymentLog(RemitaInitiatePaymentResponse response, RRRRequestModel request)
        {
            if (response != null)
            {
                var paymentLog = new PaymentLog()
                {
                    ApplicationId = request.orderId,
                    TransactionDate = DateTime.UtcNow,
                    LastRetryDate = DateTime.UtcNow,
                    PaymentCategory = request.description,
                    TransactionId = response.RemitaInitiatePaymentStatusResponse.statuscode,
                    ApplicantId = request.payerEmail,
                    Rrreference = response.RemitaInitiatePaymentStatusResponse.RRR,
                    AppReceiptId = AppMessages.APP_RECEIPT_ID,
                    TxnAmount = Convert.ToDecimal(request.amount),
                    Arrears = 0,
                    TxnMessage = response.RemitaInitiatePaymentStatusResponse.status,
                    Account = _remitaOptions.AccountNumber,
                    BankCode = _remitaOptions.BankCode,
                    RetryCount = 0,
                    Status = AppMessages.INIT
                };
                _unitOfWork.PaymentLogRepository.Add(paymentLog);
                _unitOfWork.Complete();
            }
        }


        //public ActionResult GenerateRRRs(string ApplicationId, string Amount)
        //{
        //    string resultrrr = "";
        //    decimal amt = 0;
        //    var checkRRRExit = (from a in _context.PaymentLog where a.ApplicationId == ApplicationId select a).FirstOrDefault();
        //    var checkGovAgency = (from a in _context.ApplicationRequestForm where a.ApplicationId == ApplicationId select a).FirstOrDefault();
        //    // move to work flow state 3(documents attach) if application is from gov agency
        //    if (checkGovAgency?.AgencyId == 1)
        //    {
        //        checkGovAgency.CurrentStageId = 3;
        //        _context.SaveChanges();
        //        return RedirectToAction("DocumentUpload", new { ApplicationId = ApplicationId });
        //    }
        //    // generate RRR if it doesnt exist
        //    if (checkRRRExit == null)
        //    {
        //        amt = Convert.ToDecimal(Amount);
        //        var baseUrl = HttpContext.Request.Scheme + "://" + HttpContext.Request.Host + "" + "" + HttpContext.Request.PathBase;
        //        resultrrr = _utilityHelper.GeneratePaymentReference(ApplicationId, baseUrl, checkGovAgency.AgencyName, amt);
        //    }
        //    else
        //    {
        //        amt = Convert.ToDecimal(checkRRRExit.TxnAmount);
        //        resultrrr = checkRRRExit.Rrreference;
        //    }
        //    return RedirectToAction("ChargeSummary", new { RRR = resultrrr, applicationId = ApplicationId, amount = amt });
        //}




        public ActionResult ViewCertificate(string id)
        {
            var Host = HttpContext.Request.Scheme + "://" + HttpContext.Request.Host + "" + "" + HttpContext.Request.PathBase;

            var pdf = _helpersController.ViewCertificate(id, Host);

            return new ViewAsPdf("ViewCertificate", pdf)
            {
                PageSize = (Rotativa.AspNetCore.Options.Size?)Rotativa.Options.Size.A4
            };
        }

        public ActionResult DownloadCertificate(string id)
        {
            var Host = HttpContext.Request.Scheme + "://" + HttpContext.Request.Host + "" + "" + HttpContext.Request.PathBase;

            var pdf = _helpersController.ViewCertificate(id, Host);

            return new ViewAsPdf("ViewCertificate", pdf)
            {
                PageSize = (Rotativa.AspNetCore.Options.Size?)Rotativa.Options.Size.A4,
                FileName = id + ".pdf"
            };
        }



    }
}
