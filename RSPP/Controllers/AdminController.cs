using log4net;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Rotativa.AspNetCore;
using RSPP.Configurations;
using RSPP.Helper;
using RSPP.Helpers;
using RSPP.Models;
using RSPP.Models.DB;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Security.Policy;
using System.Threading.Tasks;
using static QRCoder.PayloadGenerator;

namespace RSPP.Controllers
{
    public class AdminController : AppUserController
    {



        public RSPPdbContext _context;
        IHttpContextAccessor _httpContextAccessor;
        public IConfiguration _configuration;
        GeneralClass generalClass = new GeneralClass();
        HelperController _helpersController;
        WorkFlowHelper _workflowHelper;
        List<UserMaster> staffJsonList = new List<UserMaster>();

        private ILog log = log4net.LogManager.GetLogger(typeof(AdminController));

        private readonly IWebHostEnvironment _hostingEnv;

        private const string SUPERVISOR = "SUPERVISOR";
        private const string REGISTRAR = "REGISTRAR";
        private const string OFFICER = "OFFICER";
        private const string COMPANY = "COMPANY";


        [Obsolete]
        public AdminController(RSPPdbContext context, IConfiguration configuration, IHttpContextAccessor httpContextAccessor, IWebHostEnvironment hostingEnv) : base(hostingEnv)
        {
            _context = context;
            _configuration = configuration;
            _httpContextAccessor = httpContextAccessor;
            _hostingEnv = hostingEnv;
            _helpersController = new HelperController(_context, _configuration, _httpContextAccessor);
            _workflowHelper = new WorkFlowHelper(_context);
        }


        public ActionResult Index()
        {
            int totalPermitCount = 0;
            int totalAppWorkedOn = 0;
            int totalCancelled = 0;
            String errorMessage = null;

            ViewBag.LoggedInRole = _helpersController.getSessionRoleName();
            ViewBag.UserGuides = this.GetUserGuides(USER_TYPE_ADMIN, USER_GUIDES_ADMIN_PATH);

            try
            {


                foreach (ApplicationRequestForm appRequest in _context.ApplicationRequestForm.ToList())
                {
                    switch (_context.WorkFlowState.Where(w => w.StateId == appRequest.CurrentStageId).FirstOrDefault().StateType)
                    {
                        case "COMPLETE":
                            totalPermitCount++;
                            break;
                        case "PROGRESS":
                            totalAppWorkedOn++;
                            break;
                        case "REJECTED":
                            totalCancelled++;
                            break;
                        default:
                            break;
                    }
                }
                log.Info("totalPermitCount =>" + totalPermitCount);
                log.Info("totalAppWorkedOn =>" + totalAppWorkedOn);
                log.Info("totalCancelled =>" + totalCancelled);

                List<ApplicationRequestForm> myAppDeskCountList = _helpersController.GetApprovalRequest(out errorMessage);
                log.Info("OnMyDeskCount =>" + myAppDeskCountList.Count);
                ViewBag.OnMyDeskCount = myAppDeskCountList.Count;
                ViewBag.TotalApplicationWorkedOn = (from a in _context.ApplicationRequestForm where a.Status == "Processing" select a).ToList().Count();
                ViewBag.PermitCount = (from a in _context.ApplicationRequestForm where a.Status == "Approved" select a).ToList().Count();
                ViewBag.TotalRejection = (from a in _context.ApplicationRequestForm where a.Status == "Rejected" select a).ToList().Count();
                ViewBag.ErrorMessage = errorMessage;
                var pastthreeweek = _helpersController.GetApplicationForPastThreeWks();
                var pastfivedays = _helpersController.GetApplicationForPastFiveDays();
                ViewBag.Pastfivedays = pastfivedays;
                ViewBag.Pastwksapp = pastthreeweek;
            }
            catch (Exception ex)
            {
                log.Error(ex.StackTrace);
                ViewBag.ErrorMessage = "Error Occured on Admin DashBoard, Please try again Later";
            }

            return View();
        }






        [HttpGet]
        [AllowAnonymous]
        public ActionResult GetChart(RatioDash ratio)
        {

            int inprogressOnline = (from a in _context.ApplicationRequestForm where a.Status == "Processing" select a).ToList().Count();
            int issued = (from a in _context.ApplicationRequestForm where a.LicenseReference != null select a).ToList().Count();
            int approved = (from a in _context.ApplicationRequestForm where a.Status == "Approved" select a).ToList().Count();
            int rejected = (from a in _context.ApplicationRequestForm where a.Status == "Rejected" select a).ToList().Count();
            ratio.Approved = approved;
            ratio.OnDesk = (from a in _context.ApplicationRequestForm where a.LastAssignedUser == _helpersController.getSessionEmail() select a).ToList().Count();
            ratio.Processed = issued;
            ratio.Rejected = rejected;
            ratio.OnlineInProgress = inprogressOnline;
            return Json(ratio);
        }







        [HttpPost]
        public async Task<IActionResult> NewUser(IFormFile MyUploader, string UserEmail, string staffFullName, string Userroles)
        {
            string status = string.Empty;
            string message = string.Empty;
            string fileName = "";
            try
            {
                var userdata = new UserMaster();
                var checkexistence = (from u in _context.UserMaster where u.UserEmail == UserEmail select u).FirstOrDefault();
                if (checkexistence != null)
                {
                    message = "Record with the email address " + checkexistence.UserEmail + " already exist in the database.";
                    status = "failed";
                }
                else
                {
                    //var splitfullname = Request.Form["staffFullName"].ToString();
                    var name = staffFullName.ToString().Split(' ');
                    if (name.Count() <= 1)
                    {
                        status = "fullnamerequired";
                        message = "Please enter full name";
                        return Json(new { Status = status, Message = message });
                    }
                    var firstname = name[0];
                    var lastname = name[1];
                    if (MyUploader != null)
                    {
                        fileName = Path.GetFileName(MyUploader.FileName);
                        var filePath = Path.Combine(_hostingEnv.WebRootPath, "images", fileName);
                        using (var fileSteam = new FileStream(filePath, FileMode.Create))
                        {
                            await MyUploader.CopyToAsync(fileSteam);
                        }
                        userdata.SignatureImage = $"{"~/images/"}{fileName}";
                    }

                    userdata.UserEmail = UserEmail;
                    userdata.UserType = "ADMIN";
                    userdata.UserRole = Userroles;
                    userdata.FirstName = firstname;
                    userdata.LastName = lastname;
                    userdata.CreatedOn = DateTime.Now;
                    userdata.UpdatedBy = _helpersController.getSessionEmail();
                    userdata.UpdatedOn = DateTime.Now;
                    userdata.Status = "ACTIVE";
                    userdata.LastLogin = DateTime.Now;
                    _context.UserMaster.Add(userdata);
                    _context.SaveChanges();


                    status = "success";
                    message = "Record was successfully added";
                }

            }
            catch (Exception ex)
            {
                status = "failed";
                message = "Something went wrong please try again later " + ex.Message;
            }


            return Json(new { Status = status, Message = message });
        }





        [HttpPost]
        public async Task<IActionResult> EditStaff(IFormFile MyFileUploader, string FirstName, string LastName, string EditUserEmail, string Userrole)
        {
            string status = string.Empty;
            string message = string.Empty;
            try
            {
                var user = (from u in _context.UserMaster where u.UserEmail == EditUserEmail select u).FirstOrDefault();
                if (MyFileUploader != null)
                {
                    var fileName = Path.GetFileName(MyFileUploader.FileName);
                    var filePath = Path.Combine(_hostingEnv.WebRootPath, "images", fileName);
                    using (var fileSteam = new FileStream(filePath, FileMode.Create))
                    {
                        await MyFileUploader.CopyToAsync(fileSteam);
                    }
                    user.SignatureImage = $"{"~/images/"}{fileName}";
                }
                user.UserRole = Userrole;
                user.FirstName = FirstName;
                user.LastName = LastName;
                _context.SaveChanges();
                status = "success";
                message = "Staff Update was Successful";
            }
            catch (Exception ex)
            {
                status = "failed";
                message = "Something went wrong " + ex.Message;
            }
            return Json(new { Status = status, Message = message });
        }





        [HttpPost]
        public JsonResult DeleteUser()
        {
            string status = string.Empty;
            string message = string.Empty;
            var email = Request.Form["useremail"].ToString();
            try
            {
                var useremail = (from u in _context.UserMaster where u.UserEmail == email select u).ToList();

                if (useremail != null)
                {
                    _context.UserMaster.Remove(useremail.FirstOrDefault());
                    _context.SaveChanges();
                }
                status = "success";
                message = email + " was successfully deleted";
            }
            catch (Exception ex)
            {
                status = "failed";
                message = ex.Message + " Unable to delete user " + email;
            }
            return Json(new { Status = status, Message = message });
        }





        [HttpPost]
        public JsonResult ActivateUser()
        {
            string status = string.Empty;
            string message = string.Empty;
            var usr = Request.Form["userID"].ToString();
            try
            {

                var deactive = Request.Form["Activate"].ToString();
                var location = Request.Form["UserLocationActivate"].ToString();
                var usermas = (from u in _context.UserMaster where u.UserEmail == usr select u).FirstOrDefault();
                usermas.Status = "ACTIVE";
                usermas.LastComment = deactive;

                _context.SaveChanges();
                status = "success";
                message = usr + " was Successfully Activated";

            }

            catch (Exception ex)
            {
                status = "failed";
                message = ex.Message + " Unable to delete user " + usr;
            }
            return Json(new { Status = status, Message = message });
        }

        [HttpPost]
        public ActionResult DeactivateUser()
        {
            string status = string.Empty;
            string message = string.Empty;
            var usr = Request.Form["userid"].ToString();

            try
            {

                var deactive = Request.Form["DeactivateComment"].ToString();
                var usermas = (from u in _context.UserMaster where u.UserEmail == usr select u).FirstOrDefault();
                usermas.Status = "PASSIVE";
                usermas.LastComment = deactive;
                _context.SaveChanges();
                status = "success";
                message = usr + " was Successfully Deactivated";

            }

            catch (Exception ex)
            {
                status = "failed";
                message = ex.Message + " Unable to deactive user " + usr;
            }
            return Json(new { Status = status, Message = message });
        }

        public JsonResult GetFees(int MyCategoryid)
        {

            var details = (from a in _context.LineOfBusiness where a.LineOfBusinessId == MyCategoryid select a).FirstOrDefault();

            return Json(new { formid = details.FormTypeId });
        }


        [AllowAnonymous]
        [HttpPost]
        public ActionResult GetAllUser()
        {
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
            var staff = (from u in _context.UserMaster
                         where u.UserType != "COMPANY"

                         select new
                         {
                             u.UserEmail,
                             fullname = u.FirstName + " " + u.LastName,
                             u.UserRole,
                             u.UserType,
                             u.SignatureImage,
                             u.Status
                         });

            if (!string.IsNullOrEmpty(searchTxt))
            {
                staff = staff.Where(u => u.fullname.Contains(searchTxt) || u.UserEmail.Contains(searchTxt)
               || u.UserRole.Contains(searchTxt) || u.UserType.Contains(searchTxt) || u.Status.Contains(searchTxt));
            }
            if (!(string.IsNullOrEmpty(sortColumn) && string.IsNullOrEmpty(sortColumnDir)))
            {
                staff = staff.OrderBy(s => s.UserEmail + " " + sortColumnDir);
            }


            totalRecords = staff.Count();
            var data = staff.Skip(skip).Take(pageSize).ToList();

            return Json(new { draw = draw, recordsFiltered = totalRecords, recordsTotal = totalRecords, data = data });

        }

        public ActionResult AllApplications()
        {
            List<MyApplicationRequestForm> apps = null;

            apps = (from a in _context.ApplicationRequestForm
                    join u in _context.UserMaster on a.CompanyEmail equals u.UserEmail
                    //join p in _context.PaymentLog on a.ApplicationId equals p.ApplicationId
                    join w in _context.WorkFlowState on a.CurrentStageId equals w.StateId
                    where a.IsLegacy == "NO"
                    orderby a.AddedDate descending
                    select new MyApplicationRequestForm
                    {
                        ApplicationId = a.ApplicationId,
                        CompanyEmail = a.CompanyEmail,
                        AmountPaid = 0,
                        CompanyName = u.CompanyName,
                        AgencyName = a.AgencyName,
                        Status = a.Status,
                        CurrentStage = w.StateName,
                        AppProgress = w.Progress,
                        CurrentStageId = a.CurrentStageId,
                        AddedDate = a.AddedDate
                    }).ToList();

            return View(apps);
        }


        public ActionResult AllLegacy()
        {
            List<ApplicationRequestForm> apps = null;

            apps = (from a in _context.ApplicationRequestForm where a.IsLegacy == "YES" orderby a.AddedDate descending select a).ToList();

            return View(apps);
        }

        public ActionResult AllExtraPayment()
        {
            List<PaymentModel> extrapayment = new List<PaymentModel>();
            var paymt = (from p in _context.ExtraPayment
                         join a in _context.ApplicationRequestForm on p.ApplicationId equals a.ApplicationId
                         select new
                         {
                             p.Rrreference,
                             p.Status,
                             p.TransactionDate,
                             p.TxnAmount,
                             a.AgencyName,
                             p.ApplicationId,
                             a.CompanyEmail,
                             p.TransactionId,
                             p.ExtraPaymentBy
                         }).ToList();

            foreach (var item in paymt)
            {
                extrapayment.Add(new PaymentModel()
                {

                    RRReference = item.Rrreference,
                    Status = item.Status,
                    TransactionDate = item.TransactionDate,
                    TxnAmount = item.TxnAmount,
                    ApplicantName = item.AgencyName,
                    ApplicationID = item.ApplicationId,
                    CompanyUserId = item.CompanyEmail,
                    TransactionID = item.TransactionId,
                    ExtraPaymentBy = item.ExtraPaymentBy
                });
            }
            ViewBag.ExtraPaymentList = extrapayment;
            return View();
        }

        public ActionResult AllPayment()
        {
            List<PaymentModel> payment = new List<PaymentModel>();
            var paymt = (from p in _context.PaymentLog
                         join a in _context.ApplicationRequestForm on p.ApplicationId equals a.ApplicationId
                         select new
                         {
                             p.Rrreference,
                             p.Status,
                             p.TransactionDate,
                             p.TxnAmount,
                             a.AgencyName,
                             p.ApplicationId,
                             a.CompanyEmail,
                             p.Arrears,
                             p.TransactionId
                         }).ToList();

            foreach (var item in paymt)
            {
                payment.Add(new PaymentModel()
                {

                    RRReference = item.Rrreference,
                    Status = item.Status,
                    TransactionDate = item.TransactionDate,
                    TxnAmount = item.TxnAmount,
                    ApplicantName = item.AgencyName,
                    ApplicationID = item.ApplicationId,
                    CompanyUserId = item.CompanyEmail,
                    Arrears = item.Arrears,
                    TransactionID = item.TransactionId
                });
            }
            ViewBag.PaymentList = payment;
            return View();
        }

        public ActionResult AllStaffOutofOffice()
        {
            ViewBag.AllStaffOutofOfficeList = _helpersController.GetAllOutofOffice();
            return View();
        }

        [HttpGet]
        public ActionResult ApplicationDetails(string applicationId)
        {
            MyApplicationRequestForm br = null;
            br = (from p in _context.ApplicationRequestForm
                  join u in _context.UserMaster on p.CompanyEmail equals u.UserEmail
                  where p.ApplicationId == applicationId
                  select new MyApplicationRequestForm
                  {
                      ApplicationId = p.ApplicationId,
                      AgencyId = p.AgencyId,
                      AgencyName = p.AgencyName,
                      CompanyAddress = p.CompanyAddress,
                      CompanyEmail = p.CompanyEmail,
                      CompanyName = u.CompanyName,
                      CompanyWebsite = p.CompanyWebsite,
                      DateofEstablishment = p.DateofEstablishment
                  }).FirstOrDefault();
            if (br != null)
            {
                ViewBag.MyAgencyId = br.AgencyId;

                if (br.AgencyId == 1)
                {
                    ViewBag.Governmentagency = _helpersController.Governmentagency(applicationId);

                }
                else if (br.AgencyId == 2)
                {
                    ViewBag.LogisticsServiceProvider = _helpersController.LogisticsServiceProvider(applicationId);

                }
                else if (br.AgencyId == 3)
                {
                    ViewBag.PortOffDockTerminalOperator = _helpersController.PortOffDockTerminalOperator(applicationId);

                }
                else if (br.AgencyId == 4)
                {
                    ViewBag.ShippingAgency = _helpersController.ShippingAgency(applicationId);

                }
                else if (br.AgencyId == 5)
                {
                    ViewBag.OtherPortServiceProvider = _helpersController.OtherPortServiceProvider(applicationId);

                }
                else if (br.AgencyId == 6)
                {
                    ViewBag.UserOfPortService = _helpersController.UserOfPortService(applicationId);

                }
            }

            return View(br);
        }

        private object FilterApplications()
        {
            return (from a in _context.ApplicationRequestForm
                    join u in _context.UserMaster on a.CompanyEmail equals u.UserEmail
                    select new
                    {
                        a.ApplicationId,
                        a.Status,
                        a.CompanyEmail,
                        u.CompanyName,
                        a.AddedDate,
                        issueddate = a.LicenseIssuedDate.ToString(),
                        expirydate = a.LicenseExpiryDate.ToString(),
                        a.LicenseExpiryDate,
                        a.LicenseIssuedDate,
                        a.AgencyName,
                        a.LineOfBusinessId,
                        a.ApplicationTypeId,
                        a.IsLegacy
                    });

        }


        public ActionResult ApplicationReport()
        {
            int totalapplication = (from a in _context.ApplicationRequestForm select a).ToList().Count();

            TempData["totalapplication"] = totalapplication;

            var applications = JsonConvert.SerializeObject(FilterApplications());
            var data = JsonConvert.DeserializeObject<List<ApplicationReportViewModel>>(applications);

            TempData["totalapplication"] = data.Count();



            return View(data);
        }


        [AllowAnonymous]
        [HttpPost]
        public ActionResult ApplicationReport(string min, string maxdate, List<string> status, List<string> cat, List<string> apptype)
        {
            var applications = JsonConvert.SerializeObject(FilterApplications());
            var data = JsonConvert.DeserializeObject<List<ApplicationReportViewModel>>(applications);


            if ((!string.IsNullOrEmpty(min) && (!string.IsNullOrEmpty(maxdate))))
            {
                var startdate = Convert.ToDateTime(min).Date;
                var enddate = Convert.ToDateTime(maxdate).Date;
                data = data.Where(a => a.AddedDate >= new DateTime(startdate.Year, startdate.Month, startdate.Day, 00, 00, 00) && a.AddedDate <= new DateTime(enddate.Year, enddate.Month, enddate.Day, 23, 59, 59)).ToList();
            }

            if (status != null && status.Count() > 0)
                data = data.Where(x => status.Contains(x.Status)).ToList();

            if (cat != null && cat.Count() > 0)
                data = data.Where(x => cat.Contains(x.LineOfBusinessId)).ToList();

            if (apptype != null && apptype.Count() > 0)
                data = data.Where(x => apptype.Contains(x.ApplicationTypeId)).ToList();

            TempData["totalapplication"] = data.Count();
            ViewData["startdate"] = min;
            ViewData["maxdate"] = maxdate;
            return View(data);
        }



        [AllowAnonymous]
        [HttpPost]
        public ActionResult GetApplicationReport()
        {

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

            var applications = JsonConvert.SerializeObject(FilterApplications());
            var staff = JsonConvert.DeserializeObject<IEnumerable<ApplicationReportViewModel>>(applications);

            if (!(string.IsNullOrEmpty(sortColumn) && string.IsNullOrEmpty(sortColumnDir)))
            {
                staff = staff.OrderBy(s => s.ApplicationID + " " + sortColumnDir);
            }
            if (!string.IsNullOrEmpty(searchTxt))
            {
                if (searchTxt == "All Company")
                {
                    staff = staff.Where(s => s.ApplicationID == s.ApplicationID);
                }
                else
                {
                    staff = staff.Where(a => a.ApplicationID.Contains(searchTxt) || a.CompanyEmail.Contains(searchTxt)
                   || a.Status.Contains(searchTxt) || a.CompanyName.Contains(searchTxt)
                   || a.issueddate.Contains(searchTxt) || a.expirydate.Contains(searchTxt));
                }
            }

            string firstdate = Request.Form["mymin"];
            string lastdate = Request.Form["mymax"];
            if ((!string.IsNullOrEmpty(firstdate) && (!string.IsNullOrEmpty(lastdate))))
            {
                var mindate = Convert.ToDateTime(firstdate).Date;
                var maxdate = Convert.ToDateTime(lastdate).Date;
                staff = staff.Where(a => a.ApplicationID == a.ApplicationID && a.issuedDATE >= mindate && a.issuedDATE <= maxdate);
            }

            totalRecords = staff.Count();
            var data = staff.Skip(skip).Take(pageSize).ToList();

            return Json(new { draw = draw, recordsFiltered = totalRecords, recordsTotal = totalRecords, data = data });

        }

        //public ActionResult ApplicationReport()
        //{
        //    int totalapplication = (from a in _context.ApplicationRequestForm select a).ToList().Count();
        //    TempData["totalapplication"] = totalapplication;
        //    return View();
        //}





        //[AllowAnonymous]
        //[HttpPost]
        //public ActionResult GetApplicationReport()
        //{
        //    var draw = Request.Form["draw"].FirstOrDefault();
        //    var start = Request.Form["start"].FirstOrDefault();
        //    var length = Request.Form["length"].FirstOrDefault();

        //    var sortColumn = Request.Form["columns[" + Request.Form["order[0][column]"].FirstOrDefault() + "][name]"].FirstOrDefault();
        //    var sortColumnDir = Request.Form["order[0][dir]"].FirstOrDefault();

        //    var searchTxt = Request.Form["search[value]"][0];

        //    int pageSize = length != null ? Convert.ToInt32(length) : 0;
        //    int skip = start != null ? Convert.ToInt32(start) : 0;
        //    int totalRecords = 0;
        //    var today = DateTime.Now.Date;
        //    var staff = (from p in _context.ApplicationRequestForm

        //                 select new
        //                 {
        //                     p.ApplicationId,
        //                     p.Status,
        //                     p.CompanyEmail,
        //                     p.AgencyName,
        //                     LicenseIssuedDate = p.LicenseIssuedDate.ToString(),
        //                     LicenseExpiryDate = p.LicenseExpiryDate.ToString(),
        //                     expiryDATE = p.LicenseExpiryDate,
        //                     issuedDATE = p.LicenseIssuedDate
        //                 });

        //    if (!(string.IsNullOrEmpty(sortColumn) && string.IsNullOrEmpty(sortColumnDir)))
        //    {
        //        staff = staff.OrderBy(s => s.ApplicationId + " " + sortColumnDir);
        //    }
        //    if (!string.IsNullOrEmpty(searchTxt))
        //    {
        //        if (searchTxt == "All Company")
        //        {
        //            staff = staff.Where(s => s.ApplicationId == s.ApplicationId);
        //        }
        //        else
        //        {
        //            staff = staff.Where(a => a.ApplicationId.Contains(searchTxt) || a.CompanyEmail.Contains(searchTxt)
        //           || a.Status.Contains(searchTxt) || a.AgencyName.Contains(searchTxt)
        //           || a.LicenseIssuedDate.Contains(searchTxt) || a.LicenseExpiryDate.Contains(searchTxt));
        //        }
        //    }
        //    string firstdate = Request.Form["mymin"];
        //    string lastdate = Request.Form["mymax"];
        //    if ((!string.IsNullOrEmpty(firstdate) && (!string.IsNullOrEmpty(lastdate))))
        //    {
        //        var mindate = Convert.ToDateTime(firstdate);
        //        var maxdate = Convert.ToDateTime(lastdate);
        //        staff = staff.Where(a => a.ApplicationId == a.ApplicationId && a.issuedDATE >= mindate && a.issuedDATE <= maxdate);
        //    }

        //    totalRecords = staff.Count();
        //    var data = staff.Skip(skip).Take(pageSize).ToList();

        //    return Json(new { draw = draw, recordsFiltered = totalRecords, recordsTotal = totalRecords, data = data });

        //}

        [HttpGet]
        public ActionResult GetApplicationChart(ApplicationRatio Appobj)
        {
            Appobj.Initiated = (from a in _context.ApplicationRequestForm where a.Status == "ACTIVE" select a).ToList().Count();
            Appobj.Approved = (from a in _context.ApplicationRequestForm where a.Status == "Approved" select a).ToList().Count();
            Appobj.Processing = (from a in _context.ApplicationRequestForm where a.Status == "Processing" select a).ToList().Count();
            Appobj.Rejected = (from a in _context.ApplicationRequestForm where a.Status == "Rejected" select a).ToList().Count();
            Appobj.Legacy = (from a in _context.ApplicationRequestForm where a.IsLegacy == "YES" select a).ToList().Count();
            return Json(Appobj);
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
                log.Info("Response from shippers council =>" + appResponse.message);
                if (appResponse.message.Trim() != "SUCCESS")
                {
                    responseMessage = "An Error occurred, Please try again Later";
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
        public ActionResult Companies()
        {
            return View(_context.UserMaster.Where(u => u.UserType == "COMPANY").ToList());
        }

        [HttpGet]
        public ActionResult CompanyDocuments(string compId)
        {

            return View(_helpersController.CompanyDocument(compId));
        }


        [HttpGet]
        public ActionResult CompanyApplications(string userId)
        {
            UserMaster up = _context.UserMaster.Where(c => c.UserEmail.Trim() == userId.Trim()).FirstOrDefault();
            var CompanyApps = (from c in _context.ApplicationRequestForm where c.CompanyEmail == userId select c).ToList();
            ViewBag.CompanyName = up.CompanyName;
            return View(CompanyApps);
        }


        [HttpGet]
        public ActionResult CompanyPermits(string userId)
        {
            var companypermit = (from a in _context.ApplicationRequestForm where a.CompanyEmail == userId && a.LicenseReference != null select a).ToList();
            ViewBag.CompanyEmail = userId;
            return View(companypermit);
        }

        [HttpGet]
        public ActionResult CompanyProfile(string CompanyEmail)
        {

            var companydetails = (from a in _context.UserMaster where a.UserEmail == CompanyEmail select a).FirstOrDefault();

            ViewBag.AllCompanyDocument = _helpersController.CompanyDocument(CompanyEmail);
            var companyProfileForUpdate = new CompanyProfileModel
            {
                UserEmail = CompanyEmail,
                EmailForUpdate = CompanyEmail,
                CompanyName = companydetails.CompanyName,
                PhoneNum = companydetails.PhoneNum,
                CompanyAddress = companydetails.CompanyAddress,
            };

            return View(companyProfileForUpdate);
        }

        [ValidateAntiForgeryToken]
        public ActionResult UpdateCompanyRecord(CompanyProfileModel model)
        {
            string status = "failed";
            string message = "unable to update company details";
            string emailResponseMessage = string.Empty;
            string actionType = Request.Form["actionType"];

            //var companyDetails = _context.UserMaster.Include(user => user.App)

            var companydetails = (from u in _context.UserMaster where u.UserEmail == model.UserEmail select u).FirstOrDefault();
            if (companydetails is null)
            {
                return Json(new
                {
                    Status = status,
                    Message = "User Not found"
                });
            }

            if (actionType.Contains("UPDATE_PROFILE"))
            {
                companydetails.CompanyName = model.CompanyName;
                companydetails.PhoneNum = model.PhoneNum;

                if (companydetails.UserEmail != model.EmailForUpdate)
                {
                    var applicationsForThisUser = _context.ApplicationRequestForm
                        .Where(app => app.CompanyEmail == companydetails.UserEmail).ToList();
                    foreach (var app in applicationsForThisUser)
                    {
                        app.CompanyEmail = model.EmailForUpdate;
                    }
                    companydetails.UserEmail = model.EmailForUpdate;

                    var emailMessage = GenerateEmailBodyForCompanyEmailUpdate(companydetails.UserEmail);

                    var emailResponse = Emailer.SendEmail(
                        companydetails.CompanyName,
                        companydetails.UserEmail,
                        "Password Reset Confirmation",
                        emailMessage);
                    if (!emailResponse.Success)
                    {
                        return Json(new
                        {
                            Status = status,
                            emailResponse.ResultMessage
                        });
                    }
                    emailResponseMessage = "A reset link has been sent to <strong>" + companydetails.UserEmail + "</strong>";
                }
            }
            else if (actionType.Contains("ADDRESS"))
            {
                companydetails.CompanyAddress = model.CompanyAddress;
            }

            //_context.UserMaster.Update(companydetails);


            if (_context.SaveChanges() > 0)
            {

                status = "success";
                message = "Company details updated. " + emailResponseMessage;
            }
            else
            {
                message = "No updates were made.";
            };

            return Json(new
            {
                Status = status,
                Message = message
            });
        }

        private string GenerateEmailBodyForCompanyEmailUpdate(string userEmail)
        {
            Random rnd = new Random();
            int value = rnd.Next(100000, 999999);
            string password = "nsc-" + value;
            string content = "Your New Password is " + password;
            password = generalClass.Encrypt(password);

            string subject = "Reset Password Activation Link";

            var msgBody = generalClass.ForgotPasswordTemplate(userEmail, subject, content, password);

            return msgBody;
        }


        public ActionResult GenerateExtraPayment(FormCollection collection)
        {
            List<ApplicationRequestForm> apps = new List<ApplicationRequestForm>();
            var extra = (from a in _context.ApplicationRequestForm where a.LicenseReference == null select a).ToList();

            foreach (var item in extra)
            {
                apps.Add(new ApplicationRequestForm()
                {
                    ApplicationId = item.ApplicationId,
                    CompanyEmail = item.CompanyEmail,
                    CompanyAddress = item.CompanyAddress,
                    AgencyName = item.AgencyName,
                    AddedDate = item.AddedDate
                });
            }
            ViewBag.ExtraPaymentStage = apps;
            return View();

        }

        [HttpPost]
        public ActionResult AddExtraPayment()
        {
            var appid = Request.Form["myappid"];
            var status = Request.Form["status"];
            try
            {
                ApplicationRequestForm appmaster = _context.ApplicationRequestForm.Where(c => c.ApplicationId.Trim() == appid).FirstOrDefault();
                var descrip = Request.Form["sanctiondescription"];
                var amount = Convert.ToDecimal(Request.Form["sanctionamount"]);
                var sanctype = Request.Form["sanctiontype"];
                string genApplicationId = generalClass.GenerateApplicationNo();
                var ExtraAmount = new ExtraPayment()
                {
                    ApplicationId = appid,
                    Description = descrip,
                    TxnAmount = amount,
                    ExtraPaymentAppRef = genApplicationId,
                    Arrears = 0,
                    LastRetryDate = DateTime.Now,
                    RetryCount = 1,
                    Status = status,
                    SanctionType = sanctype,
                    ApplicantId = appmaster.CompanyEmail,
                    ExtraPaymentBy = _helpersController.getSessionEmail()
                };
                _context.ExtraPayment.Add(ExtraAmount);
                int done = _context.SaveChanges();
                if (done > 0)
                {
                    TempData["GeneSuccess"] = "Extra payment was successfully generated for application with the reference number " + appid;
                    var extrapaymentdetails = (from e in _context.ExtraPayment where e.ApplicationId == appid select e).FirstOrDefault();
                    if (extrapaymentdetails != null)
                    {

                        var subject = "Extra Payment Generated";
                        var content = "YOU ARE REQUIRED TO MAKE EXTRA PAYMENT OF " + extrapaymentdetails.TxnAmount + " NAIRA FOR THE APPLICATION WITH REFERENCE NUMBER " + extrapaymentdetails.ApplicationId + ", AND YOUR REMITA REFRENCE NUMBER IS " + extrapaymentdetails.Rrreference;
                        var sendmail = generalClass.SendStaffEmailMessage(extrapaymentdetails.ApplicantId, subject, content);

                    }
                }
                else
                {
                    TempData["message"] = "something went wrong trying to generate extra payment";
                }
            }
            catch (Exception ex)
            {
                TempData["message"] = ex.Message;
            }
            if (status == "Pending")
            {
                return RedirectToAction("ViewApplication", "Admin", new { applicationId = appid });
            }
            else
            {
                return RedirectToAction("GenerateExtraPayment", "Admin");
            }
        }


        public ActionResult GetRelievedStaffOutofOffice()
        {
            ViewBag.ReliverStaffOutofOfficeList = _helpersController.GetReliverStaffOutofOffice(_helpersController.getSessionEmail());
            return View();
        }

        [HttpPost]
        public ActionResult GetStaffStartOutofOffice()
        {
            string status = "";
            try
            {

                var today = DateTime.Now.Date;
                List<OutofOffice> office = (from o in _context.OutofOffice where o.StartDate == today select o).ToList();
                foreach (var item in office)
                {
                    item.Status = "Started";
                }

                _context.SaveChanges();
                status = "done";
            }
            catch (Exception ex)
            {
                status = "failed";
                log.Error(ex.Message);
            }
            return Json(status);
        }

        [HttpPost]
        public ActionResult GetStaffEndOutofOffice()
        {
            string status = "";
            try
            {

                var today = DateTime.Now.Date;
                List<OutofOffice> office = (from o in _context.OutofOffice where o.EndDate <= today select o).ToList();
                foreach (var item in office)
                {
                    item.Status = "Finished";
                }
                _context.SaveChanges();
                status = "done";

            }
            catch (Exception ex)
            {
                status = "failed";
                log.Error(ex.Message);
            }
            return Json(status);
        }

        [HttpPost]
        public JsonResult EndLeave(OutofOffice office)
        {
            string status = string.Empty;
            string message = string.Empty;
            try
            {

                var Outoffice = (from u in _context.OutofOffice where u.Relieved == office.Relieved select u).FirstOrDefault();
                Outoffice.Status = "Finished";

                _context.SaveChanges();
                status = "success";
                message = office.Relieved + " Successfully Ended Leave";

            }

            catch (Exception ex)
            {
                status = "failed";
                message = "Unable to end leave!!! Something went wrong. " + ex.Message;
            }
            return Json(new { Status = status, Message = message });
        }


        public ActionResult ALLPermits()
        {
            return View();
        }


        [HttpPost]
        public ActionResult GetAllPermits()
        {
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
                         join u in _context.UserMaster on p.CompanyEmail equals u.UserEmail
                         orderby p.LicenseReference descending
                         where p.LicenseReference != null && p.IsLegacy == "NO"

                         select new
                         {
                             p.ApplicationId,
                             p.LicenseReference,
                             p.CompanyEmail,
                             p.CompanyAddress,
                             p.ApplicationTypeId,
                             p.AgencyName,
                             u.CompanyName
                         });


            if (!(string.IsNullOrEmpty(sortColumn) && string.IsNullOrEmpty(sortColumnDir)))
            {
                staff = staff.OrderBy(s => s.ApplicationId + " " + sortColumnDir);
            }
            if (!string.IsNullOrEmpty(searchTxt))
            {
                staff = staff.Where(a => a.ApplicationId.Contains(searchTxt) || a.AgencyName.Contains(searchTxt) || a.LicenseReference.Contains(searchTxt)
               || a.CompanyEmail.Contains(searchTxt) || a.ApplicationTypeId.Contains(searchTxt) || a.CompanyAddress.Contains(searchTxt));
            }
            totalRecords = staff.Count();
            var data = staff.Skip(skip).Take(pageSize).ToList();
            switch (sortColumn)
            {
                case "0":
                    data = sortColumnDir.Equals("DESC", StringComparison.CurrentCultureIgnoreCase) ? data.OrderByDescending(p => p.ApplicationId).ToList() : data.OrderBy(p => p.ApplicationId).ToList();
                    break;
                case "1":
                    data = sortColumnDir.Equals("DESC", StringComparison.CurrentCultureIgnoreCase) ? data.OrderByDescending(p => p.AgencyName).ToList() : data.OrderBy(p => p.AgencyName).ToList();
                    break;
                case "2":
                    data = sortColumnDir.Equals("DESC", StringComparison.CurrentCultureIgnoreCase) ? data.OrderByDescending(p => p.ApplicationTypeId).ToList() : data.OrderBy(p => p.ApplicationTypeId).ToList();
                    break;
                case "3":
                    data = sortColumnDir.Equals("DESC", StringComparison.CurrentCultureIgnoreCase) ? data.OrderByDescending(p => p.CompanyEmail).ToList() : data.OrderBy(p => p.CompanyEmail).ToList();
                    break;
                case "4":
                    data = sortColumnDir.Equals("DESC", StringComparison.CurrentCultureIgnoreCase) ? data.OrderByDescending(p => p.LicenseReference).ToList() : data.OrderBy(p => p.LicenseReference).ToList();
                    break;
                case "5":
                    data = sortColumnDir.Equals("DESC", StringComparison.CurrentCultureIgnoreCase) ? data.OrderByDescending(p => p.CompanyAddress).ToList() : data.OrderBy(p => p.CompanyAddress).ToList();
                    break;
            }
            return Json(new { draw = draw, recordsFiltered = totalRecords, recordsTotal = totalRecords, data = data });

        }

        [HttpPost]
        public ActionResult ApplicationAcceptDecline(string applicationId, string myaction, string mycomment)//, string DocId
        {
            string response = string.Empty;
            ResponseWrapper responseWrapper;
            log.Info("UserAction => " + myaction);
            log.Info("Applications => " + applicationId);
            log.Info("UserComment => " + mycomment);

            var email = _helpersController.getSessionEmail();


            log.Info("AcceptDecline Parameters =>" + applicationId + "_" + myaction + "_" + mycomment);
            try
            {
                ApplicationRequestForm appRequest = _context.ApplicationRequestForm.Where(c => c.ApplicationId.Trim() == applicationId.Trim()).FirstOrDefault();




                if (appRequest == default(ApplicationRequestForm))
                {
                    return Json(new
                    {
                        status = "failure",
                        Message = "Application ID with Reference " + applicationId + " Cannot be retrievd from the Database"
                    });
                }

                var approcom = mycomment == "" ? "" : "Comment => " + mycomment + ";";
                log.Info("Continuing with the Approval to Process Application");
                responseWrapper = _workflowHelper.processAction(appRequest.ApplicationId, myaction, email, (mycomment == "=> " || mycomment == "") ? "Application was proccessed by " + email : mycomment);
                if (!responseWrapper.status)
                {
                    log.Error(responseWrapper.value);
                    response = "You are not authorized to process this application!";
                    return Json(new
                    {
                        status = "failure",
                        Message = response
                    });
                }


            }
            catch (Exception ex)
            {
                log.Error(ex.Message);
                return Json(new { status = "failure", Message = "An Exception occur during Transaction, Please try again Later" });
            }

            return Json(new { status = "success", Message = responseWrapper.value });
        }



        [HttpGet]

        public ActionResult WorkFlow()
        {
            ViewBag.UserLoginEmail = _helpersController.getSessionRoleName();
            return View(_helpersController.GetWorkFlow());
        }

        [HttpPost]
        public JsonResult UpdateWorkFlowRecord(int WorkFlowId, string Action, string ActionRole, short CurrentStageID, short NextStateID, string TargetRole)
        {
            string result = _helpersController.UpdateWorkFlowRecord(WorkFlowId, Action, ActionRole, CurrentStageID, NextStateID, TargetRole);
            return Json(result);
        }

        [HttpPost]
        public JsonResult AddWorkFlowRecord(string Action, string ActionRole, short CurrentStageID, short NextStateID, string TargetRole)
        {
            string result = _helpersController.AddWorkFlowRecord(Action, ActionRole, CurrentStageID, NextStateID, TargetRole);
            return Json(result);
        }


        [HttpGet]
        public ActionResult WorkFlowState()
        {
            ViewBag.UserLoginEmail = _helpersController.getSessionRoleName();
            return View(_helpersController.GetWorkFlowState());
        }

        [HttpPost]
        public JsonResult UpdateWorkFlowStageRecord(short Updatestageid, string Updatecurrentstagename, string Updatecurrentstagetype, string Updatecurrentstagepercentage)
        {
            string result = _helpersController.UpdateWorkFlowStageRecord(Updatestageid, Updatecurrentstagename, Updatecurrentstagetype, Updatecurrentstagepercentage);
            return Json(result);
        }


        [HttpPost]
        public JsonResult AddWorkFlowStageRecord(string Currentstagename, string Currentstagetype, string currentstagepercentage)
        {
            string result = _helpersController.AddWorkFlowStageRecord(Currentstagename, Currentstagetype, currentstagepercentage);
            return Json(result);
        }

        public ActionResult PaymentConfig()
        {
            return View(_helpersController.GetPaymentConfig());
        }
        [HttpPost]
        public JsonResult PostPaymentConfig(string Paymentname, decimal Amount, int Formtypeid)
        {
            string result = _helpersController.AddPaymentConfig(Paymentname, Amount, Formtypeid);
            return Json(result);
        }
        [HttpPost]
        public JsonResult UpdatePaymentConfig(int UpdatepaymentId, string Updatepaymentname, decimal Updateamount, int Updateformtypeid)
        {
            string result = _helpersController.UpdatePaymentConfig(UpdatepaymentId, Updatepaymentname, Updateamount, Updateformtypeid);
            return Json(result);
        }


        public ActionResult RoleConfig()
        {
            return View(_helpersController.RoleConfig());
        }


        [HttpPost]
        public JsonResult AddRole(string Roleid, string Roledescription)
        {
            string result = _helpersController.AddRole(Roleid, Roledescription);
            return Json(result);
        }

        [HttpPost]
        public JsonResult DeleteRole(string Updateroleid)
        {
            string result = _helpersController.DeleteRole(Updateroleid);
            return Json(result);
        }
        [HttpPost]
        public JsonResult DeleteWorkFlowStage(short Stageid)
        {
            string result = _helpersController.DeleteWorkFlowStage(Stageid);
            return Json(result);
        }
        [HttpPost]
        public JsonResult DeleteWorkFlow(int Workflowid)
        {
            string result = _helpersController.DeleteWorkFlow(Workflowid);
            return Json(result);
        }
        [HttpPost]
        public JsonResult DeleteFees(int lineofbusinessid)
        {
            string result = _helpersController.DeleteFees(lineofbusinessid);
            return Json(result);
        }

        [HttpGet]
        public ActionResult GiveValueList()
        {
            return View();
        }


        [HttpPost]
        public ActionResult GetGiveValue()
        {
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
                         where p.CurrentStageId == 2
                         select new
                         {
                             p.ApplicationId,
                             p.AgencyName,
                             p.CompanyEmail,
                             p.CompanyAddress
                         });
            if (!(string.IsNullOrEmpty(sortColumn) && string.IsNullOrEmpty(sortColumnDir)))
            {
                staff = staff.OrderBy(s => s.ApplicationId + " " + sortColumnDir);
            }
            if (!string.IsNullOrEmpty(searchTxt))
            {
                staff = staff.Where(a => a.ApplicationId.Contains(searchTxt) || a.CompanyEmail.Contains(searchTxt)
               || a.AgencyName.Contains(searchTxt) || a.CompanyAddress.Contains(searchTxt));
            }
            totalRecords = staff.Count();
            var data = staff.Skip(skip).Take(pageSize).ToList();
            switch (sortColumn)
            {
                case "0":
                    data = sortColumnDir.Equals("DESC", StringComparison.CurrentCultureIgnoreCase) ? data.OrderByDescending(p => p.ApplicationId).ToList() : data.OrderBy(p => p.ApplicationId).ToList();
                    break;
                case "1":
                    data = sortColumnDir.Equals("DESC", StringComparison.CurrentCultureIgnoreCase) ? data.OrderByDescending(p => p.CompanyEmail).ToList() : data.OrderBy(p => p.CompanyEmail).ToList();
                    break;
                case "2":
                    data = sortColumnDir.Equals("DESC", StringComparison.CurrentCultureIgnoreCase) ? data.OrderByDescending(p => p.AgencyName).ToList() : data.OrderBy(p => p.AgencyName).ToList();
                    break;
                case "3":
                    data = sortColumnDir.Equals("DESC", StringComparison.CurrentCultureIgnoreCase) ? data.OrderByDescending(p => p.CompanyAddress).ToList() : data.OrderBy(p => p.CompanyAddress).ToList();
                    break;

            }
            return Json(new { draw = draw, recordsFiltered = totalRecords, recordsTotal = totalRecords, data = data });

        }

        [HttpPost]
        public ActionResult GiveValue(string Appid)
        {
            decimal processFeeAmt = 0, statutoryFeeAmt = 0;
            string errorMessage = "";
            string status = "";
            var appRequest = (from a in _context.ApplicationRequestForm where a.ApplicationId == Appid select a).FirstOrDefault();

            /// decimal Arrears = commonHelper.CalculateArrears(Appid, userMaster.UserId, dbCtxt);
            try
            {
                //errorMessage = commonHelper.GetApplicationFees(appRequest, out processFeeAmt, out statutoryFeeAmt);
                log.Info("Response Message =>" + errorMessage);


                var paylog = (from l in _context.PaymentLog where l.ApplicationId == Appid select l).FirstOrDefault();

                if (paylog == null)
                {
                    PaymentLog paymentLog = new PaymentLog();
                    paymentLog.ApplicationId = appRequest.ApplicationId;
                    paymentLog.TransactionDate = DateTime.UtcNow;
                    paymentLog.TransactionId = "Value Given";
                    paymentLog.ApplicantId = appRequest.CompanyEmail;
                    paymentLog.TxnMessage = "Given";
                    paymentLog.Rrreference = "Value Given";
                    paymentLog.AppReceiptId = "Value Given";
                    paymentLog.TxnAmount = processFeeAmt + statutoryFeeAmt;
                    paymentLog.Arrears = 0;
                    paymentLog.Account = generalClass.AccountNumberLive;
                    paymentLog.BankCode = generalClass.BankCodeLive;
                    paymentLog.RetryCount = 0;
                    paymentLog.ActionBy = _helpersController.getSessionEmail();
                    paymentLog.Status = "AUTH";
                    log.Info("About to Add Payment Log");
                    _context.PaymentLog.Add(paymentLog);
                    _context.SaveChanges();
                    log.Info("Added Payment Log to Table");
                    status = "success";
                    log.Info("Saved it Successfully");
                }
                else
                {
                    paylog.Status = "AUTH";
                    _context.SaveChanges();
                    status = "success";
                }

                if (appRequest != null)
                {
                    ResponseWrapper responseWrapper = _workflowHelper.processAction(Appid, "GenerateRRR", appRequest.CompanyEmail, "Application has moved to document upload after value given");
                }


            }
            catch (Exception ex) { ViewBag.message = ex.Message; }
            return Json(new { Status = status });
        }


        [HttpGet]
        public JsonResult GetCompanyNameAutoSearch(string term = "")
        {

            var CompanyName = (from a in _context.ApplicationRequestForm
                               where a.CompanyEmail.Contains(term)
                               group new { a.CompanyEmail } by new { a.CompanyEmail } into g
                               orderby g.Key.CompanyEmail

                               select new
                               {
                                   textvalue = g.Key.CompanyEmail,
                               }).ToList();
            return Json(CompanyName);


        }

        public ActionResult CertificateReport()
        {


            ViewBag.totalNewApp = (from s in _context.ApplicationRequestForm where s.LicenseReference != null && s.ApplicationTypeId == "NEW" select s).ToList().Count();

            ViewBag.totalReNewApp = (from s in _context.ApplicationRequestForm where s.LicenseReference != null && s.ApplicationTypeId == "RENEW" select s).ToList().Count();

            int totalLicense = (from s in _context.ApplicationRequestForm select s).ToList().Count();
            TempData["totalLicense"] = totalLicense;

            return View();
        }

        [AllowAnonymous]
        [HttpPost]
        public ActionResult GetLicenseReport()
        {
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
                         join u in _context.UserMaster on p.CompanyEmail equals u.UserEmail
                         where p.LicenseReference != null && p.LicenseReference != ""

                         select new
                         {
                             p.ApplicationId,
                             p.ApplicationTypeId,
                             p.CompanyEmail,
                             p.Status,
                             p.AgencyName,
                             u.CompanyName,
                             LicenseIssuedDate = p.LicenseIssuedDate,
                             LicenseExpiryDate = p.LicenseExpiryDate,
                             issueddate = p.LicenseIssuedDate.ToString(),
                             expirydate = p.LicenseExpiryDate.ToString()

                         });

            if (!(string.IsNullOrEmpty(sortColumn) && string.IsNullOrEmpty(sortColumnDir)))
            {
                staff = staff.OrderBy(s => s.ApplicationId + " " + sortColumnDir);
            }
            if (!string.IsNullOrEmpty(searchTxt))
            {
                if (searchTxt == "All License")
                {
                    staff = staff.Where(s => s.ApplicationId == s.ApplicationId);
                }
                else
                {
                    staff = staff.Where(a => a.ApplicationId.Contains(searchTxt) || a.CompanyEmail.Contains(searchTxt)
                    || a.Status.Contains(searchTxt) || a.AgencyName.Contains(searchTxt) || a.issueddate.Contains(searchTxt) || a.expirydate.Contains(searchTxt));

                }
            }
            string firstdate = Request.Form["mymin"];
            string lastdate = Request.Form["mymax"];
            if ((!string.IsNullOrEmpty(firstdate) && (!string.IsNullOrEmpty(lastdate))))
            {
                var mindate = Convert.ToDateTime(firstdate);
                var maxdate = Convert.ToDateTime(lastdate);
                staff = staff.Where(a => a.ApplicationId == a.ApplicationId && a.LicenseIssuedDate >= mindate && a.LicenseIssuedDate <= maxdate);
            }

            totalRecords = staff.Count();
            var data = staff.Skip(skip).Take(pageSize).ToList();

            return Json(new { draw = draw, recordsFiltered = totalRecords, recordsTotal = totalRecords, data = data });

        }

        /// <summary>
        /// Fetches applications on a user's desk
        /// </summary>
        /// <returns>A view with the users' tasks</returns>
        [HttpGet]
        public ActionResult MyDesk()
        {
            String errorMessage = null;
            var applicationsOnMyDesk = new List<MyDeskModel>();
            //List<ApplicationRequestForm> applicationsOnMyDesk = new List<ApplicationRequestForm>();
            try
            {
                //applicationsOnMyDesk = _helpersController.GetApprovalRequest(out errorMessage);
                applicationsOnMyDesk = _helpersController.GetMyDeskApplications(out errorMessage);
                log.Info("Application Returned Count =>" + applicationsOnMyDesk.Count);
                ViewBag.ErrorMessage = errorMessage;
            }
            catch (Exception ex)
            {
                log.Error(ex.StackTrace);
                ViewBag.ErrorMessage = "Error Occured when calling MyDesk, Please try again Later";
            }

            return View(applicationsOnMyDesk);
        }


        public ActionResult PrintedLicense()
        {
            return View();
        }

        public ActionResult NonPrintedLicense()
        {
            return View();
        }


        [HttpPost]
        public ActionResult GetLicensePrinted()
        {
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
                         where p.LicenseReference != null && p.PrintedStatus == "Printed"
                         select new
                         {
                             p.ApplicationId,
                             p.LicenseReference,
                             p.CompanyEmail,
                             p.CompanyAddress,
                             p.ApplicationTypeId,
                             p.AgencyName

                         });

            if (!(string.IsNullOrEmpty(sortColumn) && string.IsNullOrEmpty(sortColumnDir)))
            {
                staff = staff.OrderBy(s => s.ApplicationId + " " + sortColumnDir);
            }
            if (!string.IsNullOrEmpty(searchTxt))
            {
                staff = staff.Where(a => a.ApplicationId.Contains(searchTxt) || a.LicenseReference.Contains(searchTxt) || a.AgencyName.Contains(searchTxt)
               || a.CompanyEmail.Contains(searchTxt) || a.ApplicationTypeId.Contains(searchTxt) || a.CompanyAddress.Contains(searchTxt));
            }
            totalRecords = staff.Count();
            var data = staff.Skip(skip).Take(pageSize).ToList();
            switch (sortColumn)
            {
                case "0":
                    data = sortColumnDir.Equals("DESC", StringComparison.CurrentCultureIgnoreCase) ? data.OrderByDescending(p => p.ApplicationId).ToList() : data.OrderBy(p => p.ApplicationId).ToList();
                    break;
                case "1":
                    data = sortColumnDir.Equals("DESC", StringComparison.CurrentCultureIgnoreCase) ? data.OrderByDescending(p => p.AgencyName).ToList() : data.OrderBy(p => p.AgencyName).ToList();
                    break;
                case "2":
                    data = sortColumnDir.Equals("DESC", StringComparison.CurrentCultureIgnoreCase) ? data.OrderByDescending(p => p.CompanyEmail).ToList() : data.OrderBy(p => p.CompanyEmail).ToList();
                    break;
                case "3":
                    data = sortColumnDir.Equals("DESC", StringComparison.CurrentCultureIgnoreCase) ? data.OrderByDescending(p => p.ApplicationTypeId).ToList() : data.OrderBy(p => p.ApplicationTypeId).ToList();
                    break;
                case "4":
                    data = sortColumnDir.Equals("DESC", StringComparison.CurrentCultureIgnoreCase) ? data.OrderByDescending(p => p.CompanyAddress).ToList() : data.OrderBy(p => p.CompanyAddress).ToList();
                    break;
                case "5":
                    data = sortColumnDir.Equals("DESC", StringComparison.CurrentCultureIgnoreCase) ? data.OrderByDescending(p => p.LicenseReference).ToList() : data.OrderBy(p => p.LicenseReference).ToList();
                    break;
            }
            return Json(new { draw = draw, recordsFiltered = totalRecords, recordsTotal = totalRecords, data = data });

        }

        [HttpPost]
        public ActionResult GetNonPrintedLicense()
        {
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
                         where p.LicenseReference != null && p.PrintedStatus == "Not Printed"
                         select new
                         {
                             p.ApplicationId,
                             p.LicenseReference,
                             p.CompanyEmail,
                             p.CompanyAddress,
                             p.ApplicationTypeId,
                             p.AgencyName
                         });
            if (!(string.IsNullOrEmpty(sortColumn) && string.IsNullOrEmpty(sortColumnDir)))
            {
                staff = staff.OrderBy(s => s.ApplicationId + " " + sortColumnDir);
            }
            if (!string.IsNullOrEmpty(searchTxt))
            {
                staff = staff.Where(a => a.ApplicationId.Contains(searchTxt) || a.LicenseReference.Contains(searchTxt) || a.AgencyName.Contains(searchTxt)
               || a.CompanyEmail.Contains(searchTxt) || a.ApplicationTypeId.Contains(searchTxt) || a.CompanyAddress.Contains(searchTxt));
            }
            totalRecords = staff.Count();
            var data = staff.Skip(skip).Take(pageSize).ToList();
            switch (sortColumn)
            {
                case "0":
                    data = sortColumnDir.Equals("DESC", StringComparison.CurrentCultureIgnoreCase) ? data.OrderByDescending(p => p.ApplicationId).ToList() : data.OrderBy(p => p.ApplicationId).ToList();
                    break;
                case "1":
                    data = sortColumnDir.Equals("DESC", StringComparison.CurrentCultureIgnoreCase) ? data.OrderByDescending(p => p.AgencyName).ToList() : data.OrderBy(p => p.AgencyName).ToList();
                    break;
                case "2":
                    data = sortColumnDir.Equals("DESC", StringComparison.CurrentCultureIgnoreCase) ? data.OrderByDescending(p => p.CompanyEmail).ToList() : data.OrderBy(p => p.CompanyEmail).ToList();
                    break;
                case "3":
                    data = sortColumnDir.Equals("DESC", StringComparison.CurrentCultureIgnoreCase) ? data.OrderByDescending(p => p.ApplicationTypeId).ToList() : data.OrderBy(p => p.ApplicationTypeId).ToList();
                    break;
                case "4":
                    data = sortColumnDir.Equals("DESC", StringComparison.CurrentCultureIgnoreCase) ? data.OrderByDescending(p => p.CompanyAddress).ToList() : data.OrderBy(p => p.CompanyAddress).ToList();
                    break;
                case "5":
                    data = sortColumnDir.Equals("DESC", StringComparison.CurrentCultureIgnoreCase) ? data.OrderByDescending(p => p.LicenseReference).ToList() : data.OrderBy(p => p.LicenseReference).ToList();
                    break;
            }
            return Json(new { draw = draw, recordsFiltered = totalRecords, recordsTotal = totalRecords, data = data });

        }

        public ActionResult OutOfOffice()
        {
            ViewBag.UserID = _helpersController.getSessionEmail();

            staffJsonList = _context.UserMaster.Where(s => s.UserType != "COMPANY").ToList();
            return View();
        }

        [HttpPost]
        public JsonResult AddOutofOffice()//OutofOffice usr
        {
            string status = string.Empty;
            string message = string.Empty;

            try
            {
                var enddate = Convert.ToDateTime(Request.Form["EndDate"].ToString());
                var startdate = Convert.ToDateTime(Request.Form["StartDate"].ToString());
                var today = DateTime.Now;
                var outofoffice = new OutofOffice()
                {
                    Reliever = Request.Form["Reliever"].ToString(),
                    Relieved = Request.Form["Relieved"].ToString(),
                    EndDate = enddate,
                    StartDate = startdate,
                    Comment = Request.Form["Comment"].ToString()
                };
                outofoffice.Status = startdate < today ? "Started" : "Starting";
                _context.OutofOffice.Add(outofoffice);
                _context.SaveChanges();
                status = "success";
                message = "Successfully added out of office";

            }
            catch (Exception ex)
            {
                status = "failed";
                message = "Something went wrong " + ex.Message;
            }

            return Json(new { Status = status, Message = message });
        }

        [HttpPost]
        public JsonResult DeleteOutofOffice()//OutofOffice office
        {
            string status = string.Empty;
            string message = string.Empty;

            var relievedstaff = Request.Form["Relieved"].ToString();

            try
            {
                var useremail = (from u in _context.OutofOffice where u.Relieved == relievedstaff select u).ToList();
                if (useremail != null)
                {
                    _context.OutofOffice.Remove(useremail.FirstOrDefault());
                    _context.SaveChanges();
                }
                status = "success";
                message = "Out of office was successfully deleted";
            }
            catch (Exception ex)
            {
                status = "failed";
                message = ex.Message + " Unable to delete user " + relievedstaff;
            }
            return Json(new { Status = status, Message = message });
        }

        [HttpPost]
        public JsonResult EditOutofOffice()//OutofOffice usr
        {
            string status = string.Empty;
            string message = string.Empty;
            var relievedstaff = Request.Form["Relieved"].ToString();

            try
            {

                var outofoffice = (from u in _context.OutofOffice where u.Relieved == relievedstaff select u).FirstOrDefault();
                outofoffice.Reliever = Request.Form["Reliever"].ToString();
                outofoffice.Relieved = relievedstaff;
                outofoffice.EndDate = Convert.ToDateTime(Request.Form["EndDate"].ToString());
                outofoffice.StartDate = Convert.ToDateTime(Request.Form["StartDate"].ToString());
                outofoffice.Comment = Request.Form["Comment"].ToString();
                _context.SaveChanges();
                status = "success";
                message = "Update was Successful";

            }
            catch (Exception ex)
            {
                status = "failed";
                TempData["message"] = ex.Message + " Unable to update record";
            }

            return Json(new { Status = status, Message = message });
        }

        [AllowAnonymous]
        [HttpPost]
        public ActionResult GetOutofOfficeStaff()
        {
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
            var staff = (from u in _context.OutofOffice
                         select new
                         {
                             u.Reliever,
                             u.Relieved,
                             StartDate = u.StartDate.ToString(),
                             EndDate = u.EndDate.ToString(),
                             u.Comment,
                             u.Status
                         });

            if (!string.IsNullOrEmpty(searchTxt))
            {
                staff = staff.Where(u => u.Reliever.Contains(searchTxt) || u.Relieved.Contains(searchTxt)
               || u.EndDate.Contains(searchTxt) || u.StartDate.Contains(searchTxt) || u.Comment.Contains(searchTxt) || u.Status.Contains(searchTxt));
            }
            if (!(string.IsNullOrEmpty(sortColumn) && string.IsNullOrEmpty(sortColumnDir)))
            {
                staff = staff.OrderBy(s => s.Reliever + " " + sortColumnDir);
            }


            totalRecords = staff.Count();
            var data = staff.Skip(skip).Take(pageSize).ToList();

            return Json(new { draw = draw, recordsFiltered = totalRecords, recordsTotal = totalRecords, data = data });

        }

        [HttpGet]
        public ActionResult UserIdAutosearch(string term = "")
        {
            List<UserMaster> staffJsonList1 = new List<UserMaster>();

            foreach (UserMaster staff in _context.UserMaster.Where(s => s.UserEmail.ToLower().Contains(term.ToLower()) && s.UserRole != "COMPANY" && s.UserRole != "SUPERADMIN" && s.UserRole != "ICT"))
            {
                staffJsonList1.Add(new UserMaster() { UserEmail = staff.UserEmail, FirstName = staff.FirstName + " " + staff.LastName, UserRole = staff.UserRole });
            }

            log.Info("Fetched Staff Count =>" + staffJsonList.Count);
            return Json(staffJsonList1);

        }

        public ActionResult PaymentList()
        {
            PaymentChart chart = new PaymentChart();
            var mypayment = (from s in _context.PaymentLog where s.Status == "AUTH" select s).ToList();
            var totalpayment = mypayment.Sum(s => s.TxnAmount);
            decimal Sumtotalpayment = Convert.ToDecimal(totalpayment);
            TempData["totalpaymentcount"] = mypayment.ToList().Count();
            TempData["Sumtotalpayment"] = Sumtotalpayment.ToString("N") + " (" + generalClass.NumberToWords(Convert.ToInt64(Sumtotalpayment)) + ")";
            var paymentlist = _helpersController.PaymentChartList(chart);
            ViewBag.BargeOperators = Convert.ToDecimal(paymentlist.Bargo_Operators).ToString("N");
            ViewBag.CargoConsolidatorsDeConsolidators = Convert.ToDecimal(paymentlist.CargoConsolidators_DeConsolidators).ToString("N");
            ViewBag.Chandling = Convert.ToDecimal(paymentlist.Chandling).ToString("N");
            ViewBag.DryPortOperator = Convert.ToDecimal(paymentlist.DryPortOperator).ToString("N");
            ViewBag.FreightForwardersClearingAgents = Convert.ToDecimal(paymentlist.FreightForwarders_ClearingAgents).ToString("N");
            ViewBag.HaulersTruckers = Convert.ToDecimal(paymentlist.Haulers_Truckers).ToString("N");
            ViewBag.ICD = Convert.ToDecimal(paymentlist.ICD).ToString("N");
            ViewBag.LogististicsServiceProvider = Convert.ToDecimal(paymentlist.Logististics_Service_Provider).ToString("N");
            ViewBag.StevedoringWarehousing = Convert.ToDecimal(paymentlist.Stevedoring_Warehousing).ToString("N");
            ViewBag.SeaportTerminalOperator = Convert.ToDecimal(paymentlist.SeaportTerminalOperator).ToString("N");
            ViewBag.OffDockTerminalOperator = Convert.ToDecimal(paymentlist.OffDockTerminalOperator).ToString("N");
            ViewBag.ShippersAssociation = Convert.ToDecimal(paymentlist.ShippersAssociation).ToString("N");
            ViewBag.ShippingAgency = Convert.ToDecimal(paymentlist.ShippingAgency).ToString("N");
            ViewBag.ShippingLine = Convert.ToDecimal(paymentlist.ShippingCompany_Line).ToString("N");
            ViewBag.CargoSurveyor = Convert.ToDecimal(paymentlist.CargoSurveyor).ToString("N");
            ViewBag.IndividualCategory = Convert.ToDecimal(paymentlist.IndividualCategory).ToString("N");
            ViewBag.CorperateCategory = Convert.ToDecimal(paymentlist.CorperateCategory).ToString("N");
            ViewBag.OtherPortServiceProviders = Convert.ToDecimal(paymentlist.OtherPortServiceProviders).ToString("N");
            ViewBag.GrandTotal = Math.Round(Convert.ToDecimal(ViewBag.BargeOperators) + Convert.ToDecimal(ViewBag.CargoConsolidatorsDeConsolidators) + Convert.ToDecimal(ViewBag.Chandling) + Convert.ToDecimal(ViewBag.DryPortOperator) + Convert.ToDecimal(ViewBag.FreightForwardersClearingAgents) + Convert.ToDecimal(ViewBag.HaulersTruckers) + Convert.ToDecimal(ViewBag.ICD)
                + Convert.ToDecimal(ViewBag.LogististicsServiceProvider) + Convert.ToDecimal(ViewBag.StevedoringWarehousing) + Convert.ToDecimal(ViewBag.SeaportTerminalOperator) + Convert.ToDecimal(ViewBag.OffDockTerminalOperator) + Convert.ToDecimal(ViewBag.ShippersAssociation)
                + Convert.ToDecimal(ViewBag.CargoSurveyor) + Convert.ToDecimal(ViewBag.IndividualCategory) + Convert.ToDecimal(ViewBag.CorperateCategory) + Convert.ToDecimal(ViewBag.OtherPortServiceProviders), 2);
            //ViewBag.GrandTotal = GrandTotal.ToString("N");
            ViewBag.GrandTotalInWords = generalClass.NumberToWords(Convert.ToInt64(ViewBag.GrandTotal));
            return View();
        }


        public JsonResult PaymentChartReport()
        {
            PaymentChart paymentchart = new PaymentChart();
            paymentchart = _helpersController.PaymentChartList(paymentchart);
            return Json(paymentchart);
        }


        public ActionResult GetLicenseChart()
        {
            PaymentChart certificateobj = new PaymentChart();
            certificateobj = _helpersController.CertificateChartList(certificateobj);
            return Json(certificateobj);
        }


        public ActionResult LegacyApplicationForm()
        {
            return View();
        }

        [HttpPost]
        public JsonResult LegacyApplicationForm(MyApplicationRequestForm model, List<Terminal> MyTerminals)
        {

            var generatedapplicationid = generalClass.GenerateApplicationNo();
            var status = _helpersController.ApplicationForm(model, MyTerminals, "YES", generatedapplicationid);
            if (status == "success")
            {
                string subject = "Legacy Application Added";
                string content = "Your legacy application with the certificate reference number " + model.LicenseReference + " was successfully added to the system.";
                generalClass.SendStaffEmailMessage(model.CompanyEmail, subject, content);
            }
            return Json(new { Status = status });


        }

        [HttpPost]
        public ActionResult GetPaymentReport()
        {

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


            var staff = (from p in _context.PaymentLog

                         join r in _context.ApplicationRequestForm on p.ApplicationId equals r.ApplicationId
                         join u in _context.UserMaster on r.CompanyEmail equals u.UserEmail
                         where p.Status == "AUTH"

                         select new
                         {
                             p.ApplicationId,
                             Status = p.Status == "AUTH" ? "Paid" : "Pending",
                             p.Rrreference,
                             PaymentId = p.PaymentId.ToString(),
                             TxnAmount = p.TxnAmount.ToString(),
                             TransactionDate = p.TransactionDate.ToString(),
                             transDATE = p.TransactionDate,
                             r.ApplicationTypeId,
                             AgencyName = r.AgencyName,
                             u.CompanyName,
                             CompanyEmail = r.CompanyEmail

                         });


            if (!(string.IsNullOrEmpty(sortColumn) && string.IsNullOrEmpty(sortColumnDir)))
            {
                staff = staff.OrderBy(s => s.ApplicationId + " " + sortColumnDir);
            }
            if (searchTxt == "All Payment")
            {
                staff = staff.Where(s => s.ApplicationId == s.ApplicationId);
            }
            else
            {
                staff = staff.Where(a => a.ApplicationId.Contains(searchTxt) || a.Status.Contains(searchTxt) || a.Rrreference.Contains(searchTxt)
               || a.Status.Contains(searchTxt) || a.ApplicationTypeId.Contains(searchTxt) || a.CompanyEmail.Contains(searchTxt) || a.AgencyName.Contains(searchTxt) || a.PaymentId.Contains(searchTxt) || a.TxnAmount.Contains(searchTxt)
               || a.TransactionDate.Contains(searchTxt));
            }


            string firstdate = Request.Form["mymin"];
            string lastdate = Request.Form["mymax"];
            if ((!string.IsNullOrEmpty(firstdate) && (!string.IsNullOrEmpty(lastdate))))
            {
                var mindate = Convert.ToDateTime(firstdate).Date;
                var maxdate = Convert.ToDateTime(lastdate).Date;
                staff = staff.Where(a => a.ApplicationId == a.ApplicationId && a.transDATE >= mindate && a.transDATE <= maxdate);
            }

            totalRecords = staff.Count();
            var data = staff.Skip(skip).Take(pageSize).ToList();

            return Json(new { draw = draw, recordsFiltered = totalRecords, recordsTotal = totalRecords, data = data });

        }

        /// <summary>
        /// Fetches the details for staff (with lower hierarchy) together with the number of applications on their desk
        /// </summary>
        /// <returns>A view with the staff desk model</returns>
        public ActionResult StaffDesk()
        {
            var model = new StaffDeskModel();
            model.StaffDeskList = new List<StaffDesk>();

            var currentAdminRole = _helpersController.getSessionRoleName();
            if (currentAdminRole != OFFICER && currentAdminRole != COMPANY)
            {

                IQueryable<UserMaster> query = _context.UserMaster;

                if (currentAdminRole == SUPERVISOR)
                    query = query.Where(q => q.UserRole == OFFICER);

                if (currentAdminRole == REGISTRAR)
                    query = query.Where(q => q.UserRole == SUPERVISOR || q.UserRole == OFFICER);

                foreach (var staff in query.ToList())
                {
                    var onDesk = _context.ApplicationRequestForm
                        .Where(app => app.LastAssignedUser == staff.UserEmail).Count();

                    model.StaffDeskList.Add(new StaffDesk()
                    {
                        Role = staff.UserRole,
                        StaffEmail = staff.UserEmail,
                        StaffName = staff.FirstName,
                        status = staff.Status,
                        OnDesk = onDesk
                    });
                }

            }

            ViewBag.UserRole = currentAdminRole;
            return View(model);
        }

        //[Authorize ]
        //[HttpGet]
        //public ActionResult StaffDesk1()
        //{
        //    var userMaster = (from u in _context.UserMaster select u).FirstOrDefault();
        //    string ErrorMessage = "";
        //    StaffDeskModel model = new StaffDeskModel();
        //    List<StaffDesk> staffDeskList = new List<StaffDesk>();
        //    // select all users who are not company, superadmin, & ict
        //    var desk = _context
        //        .UserMaster.Where(a => a.UserRole != "COMPANY" && a.UserRole != "SUPERADMIN" && a.UserRole != "ICT")
        //        .ToList();
        //    //if (userMaster.UserRole != "Officer")
        //    //{

        //    //}
        //    //else
        //    //{
        //    //    ErrorMessage = "You do not have access to view this page as an Officer";
        //    //    ViewBag.ErrorMessage = ErrorMessage;    

        //    //}

        //    foreach (UserMaster up in desk)
        //    {
        //        staffDeskList.Add(new StaffDesk()
        //        {
        //            Role = up.UserRole,
        //            StaffEmail = up.UserEmail,
        //            StaffName = up.FirstName,
        //            status = up.Status,
        //            OnDesk = (from a in _context.ApplicationRequestForm where a.LastAssignedUser == up.UserEmail select a).ToList().Count()
        //        });

        //    }

        //    model.StaffDeskList = staffDeskList;
        //    ViewBag.ErrorMessage = ErrorMessage;
        //    ViewBag.UserRole = _helpersController.getSessionRoleName();
        //    return View(model);
        //}

        [HttpGet]
        public ActionResult StaffTaskAssignment(string userid, string role)
        {
            string ErrorMessage = "";
            ViewBag.UserId = userid;
            ViewBag.Rolestaff = role;
            List<ApplicationRequestForm> appRequestList;
            UserMaster up = _context.UserMaster.Where(c => c.UserEmail.Trim() == userid.Trim()).FirstOrDefault();
            appRequestList = _helpersController.GetApprovalRequest(out ErrorMessage);
            ViewBag.ErrorMessage = ErrorMessage;
            return PartialView("TaskAssignment", appRequestList);
        }

        [HttpPost]
        public ActionResult Rerouteuser()
        {
            try
            {
                var arrayitem = Request.Form["arrayitem"].ToString();
                var adcomments = Request.Form["adcomment"];
                string[] itemlist = arrayitem.Split(',');
                var newassigned = Request.Form["newassigned"];
                if (arrayitem == "" || arrayitem == null)
                {
                    TempData["Message"] = "Application(s) to be Assigned Was not Selected";
                }
                else if (newassigned == "")
                {
                    TempData["Message"] = "Please Select New Assigned User";
                }
                else
                {

                    foreach (var item in itemlist)
                    {
                        var apprequest = (from a in _context.ApplicationRequestForm where a.ApplicationId == item select a).FirstOrDefault();
                        var apphistry = (from a in _context.ActionHistory where a.ApplicationId == item select a).ToList().LastOrDefault();

                        if (apprequest != null)
                        {
                            apprequest.LastAssignedUser = newassigned;
                            apprequest.ModifiedDate = DateTime.Now;
                            _context.SaveChanges();

                            /* this is no more required*/
                            //var subject = "Re-Routed Application";
                            //string content = "Application with the reference number " + apprequest.ApplicationId + " has been re-routed to your desk.";
                            //generalClass.SendStaffEmailMessage(newassigned, subject, content);

                        }
                        if (apphistry != null)
                        {
                            string v = adcomments == "" ? "Application was Re-Assigned to " + newassigned : adcomments.ToString();
                            apphistry.Message = v;
                            _context.SaveChanges();
                        }
                    }

                }
            }
            catch (Exception ex)
            {
                TempData["Message"] = ex.Message;
            }

            return RedirectToAction("StaffDesk");

        }

        [HttpGet]
        public JsonResult AutoSearchCompanyAppId(string term = "")
        {

            var CompanyAppId = (from a in _context.PaymentLog where a.ApplicationId.Contains(term) select new { textvalue = a.ApplicationId }).ToList();
            return Json(CompanyAppId);
        }

        [HttpGet]
        public ActionResult StaffMaintenance()
        {

            //staffJsonList = _context.UserMaster.Where(s => s.UserType != "COMPANY").ToList();

            return View();
        }

        [HttpGet]
        public JsonResult GetUserRole()
        {

            var userRole = (from r in _context.Role
                            where r.RoleId != "COMPANY"
                            select new
                            {
                                role = r.RoleId
                            }).ToList();
            return Json(userRole);

        }

        [HttpGet]
        public ActionResult StaffReport()
        {
            int totalstaff = (from s in _context.UserMaster where s.UserType != "COMPANY" select s).ToList().Count();
            TempData["totalstaff"] = totalstaff;

            return View();
        }

        [HttpPost]
        public ActionResult GetStaffReport()
        {
            var role = TempData["role"];
            var field = TempData["field"];
            var zone = TempData["zone"];
            var draw = Request.Form["draw"].FirstOrDefault();
            var start = Request.Form["start"].FirstOrDefault();
            var length = Request.Form["length"].FirstOrDefault();

            var sortColumn = Request.Form["columns[" + Request.Form["order[0][column]"].FirstOrDefault() + "][name]"].FirstOrDefault();
            var sortColumnDir = Request.Form["order[0][dir]"].FirstOrDefault();

            var searchTxt = Request.Form["search[value]"][0];

            int pageSize = length != null ? Convert.ToInt32(length) : 0;
            int skip = start != null ? Convert.ToInt32(start) : 0;
            int totalRecords = 0;

            var staff = (from u in _context.UserMaster
                         where u.UserType != "COMPANY"
                         select new
                         {
                             u.UserEmail,
                             Fullname = u.FirstName + " " + u.LastName,
                             u.UserRole,
                             u.Status,

                         });
            if (!(string.IsNullOrEmpty(sortColumn) && string.IsNullOrEmpty(sortColumnDir)))
            {
                staff = staff.OrderBy(u => u.UserEmail + " " + sortColumnDir);
            }

            if (!string.IsNullOrEmpty(searchTxt))
            {
                if (searchTxt == "ALLRoles")
                {
                    staff = staff.Where(s => s.UserRole == s.UserRole);
                }
                else
                {
                    staff = staff.Where(u => u.UserEmail.Contains(searchTxt) || u.Fullname.Contains(searchTxt)
                   || u.UserRole.Contains(searchTxt) || u.Status.Contains(searchTxt));

                }
            }

            totalRecords = staff.Count();
            var data = staff.Skip(skip).Take(pageSize).ToList();
            return Json(new { draw = draw, recordsFiltered = totalRecords, recordsTotal = totalRecords, data = data });

        }

        [HttpPost]
        public JsonResult GetNewAssignedUser(string myrole, string myoldemail)
        {

            var Newuseremail = (from r in _context.UserMaster
                                where r.UserRole == myrole && r.Status == "ACTIVE" && r.UserEmail != myoldemail
                                select new
                                {
                                    newuser = r.UserEmail
                                }).ToList();
            return Json(Newuseremail);
        }

        [HttpPost]
        public ActionResult TaskDelegationList()
        {
            var draw = Request.Form["draw"].FirstOrDefault();
            var start = Request.Form["start"].FirstOrDefault();
            var length = Request.Form["length"].FirstOrDefault();
            var sortColumn = Request.Form["columns[" + Request.Form["order[0][column]"].FirstOrDefault() + "][name]"].FirstOrDefault();
            var sortColumnDir = Request.Form["order[0][dir]"].FirstOrDefault();

            var searchTxt = Request.Form["search[value]"][0];

            int pageSize = length != null ? Convert.ToInt32(length) : 0;
            int skip = start != null ? Convert.ToInt32(start) : 0;
            int totalRecords = 0;
            string staffemail = Request.Form["mystaffemail"];

            var Applications = (from a in _context.ApplicationRequestForm
                                where a.Status == "Processing" && staffemail.Contains(a.LastAssignedUser)
                                select new { a.ApplicationId, a.AgencyName, a.ApplicationTypeId, a.CompanyEmail, a.LastAssignedUser });

            if (!string.IsNullOrEmpty(searchTxt))
            {
                Applications = Applications.Where(s => s.ApplicationId.Contains(searchTxt) || s.AgencyName.Contains(searchTxt) || s.ApplicationTypeId.Contains(searchTxt) || s.LastAssignedUser.Contains(searchTxt) || s.CompanyEmail.Contains(searchTxt));
            }
            if (!(string.IsNullOrEmpty(sortColumn) && string.IsNullOrEmpty(sortColumnDir)))
            {
                Applications = Applications.OrderBy(p => p.ApplicationId + " " + sortColumnDir);
            }


            totalRecords = Applications.Count();
            var data = Applications.Skip(skip).Take(pageSize).ToList();

            return Json(new { draw = draw, recordsFiltered = totalRecords, recordsTotal = totalRecords, data = data });

        }

        [HttpGet]
        public ActionResult ViewStaffDesk(string userid)
        {
            string ErrorMessage = "";
            ViewBag.UserId = userid;
            List<ApplicationRequestForm> appRequestList;
            UserMaster up = _context.UserMaster.Where(c => c.UserEmail.Trim() == userid.Trim()).FirstOrDefault();
            appRequestList = _helpersController.GetApprovalRequests(up, out ErrorMessage);
            ViewBag.ErrorMessage = ErrorMessage;

            return PartialView("_StaffDesk", appRequestList);
        }

        [HttpGet]
        public ActionResult TransitionHistory(string applicationId)
        {

            List<ActionHistory> NotificationList = new List<ActionHistory>();
            try
            {
                NotificationList = _context.ActionHistory.Where(a => a.ApplicationId == applicationId).ToList();
                ViewBag.ApplicationID = applicationId;
                ViewBag.ErrorMessage = "SUCCESS";
            }
            catch (Exception ex)
            {
                log.Error(ex.StackTrace);
                ViewBag.ErrorMessage = "Error Occured when calling Transition History, Please try again Later";
            }

            return View(NotificationList);
        }


        public ActionResult DocumentConfig()
        {

            List<DocumentList> doclist = null;
            doclist = (from d in _context.Documents
                       select new DocumentList
                       {
                           CompulsoryDocument = d.IsMandatory == "Y" ? "YES" : "NO",
                           DocumentName = d.DocumentName,
                           DocId = d.DocId,
                           CategoryName = (from l in _context.LineOfBusiness where l.LineOfBusinessId == d.LineOfBusinessId select l.LineOfBusinessName).FirstOrDefault()
                       }).ToList();
            return View(doclist);
        }


        [HttpPost]
        public JsonResult AddDocument(string docname, List<string> DocCategory, string Doccompulsory)
        {
            string status = string.Empty;
            string message = string.Empty;
            try
            {
                var docs = new Documents();
                if (DocCategory.Count > 0)
                {
                    foreach (var item in DocCategory)
                    {
                        docs.DocId = 0;
                        docs.DocumentName = docname;
                        docs.IsMandatory = Doccompulsory;
                        docs.LineOfBusinessId = Convert.ToInt32(item);
                        _context.Documents.Add(docs);
                        _context.SaveChanges();
                    }

                }
                status = "success";
                message = "document was successfully added";
            }
            catch (Exception ex)
            {
                status = "failed";
                message = "Unable to add document. Please try again later " + ex.InnerException;
            }
            return Json(new { Status = status, Message = message });
        }

        public JsonResult GetAllCategory()
        {
            var Category = (from l in _context.LineOfBusiness select new { l.LineOfBusinessId, l.LineOfBusinessName }).ToList();
            return Json(Category);
        }

        [HttpPost]
        public JsonResult DeleteDocument(int docid)
        {
            string status = string.Empty;
            string message = string.Empty;

            try
            {
                var deletedoc = (from d in _context.Documents where d.DocId == docid select d).FirstOrDefault();
                _context.Remove(deletedoc);
                _context.SaveChanges();
                status = "success";
                message = "Document was successfully deleted";
            }
            catch (Exception ex)
            {
                status = "failed";
                message = "Unable to deleted document. Please try again later " + ex.InnerException;
            }

            return Json(new { Status = status, Message = message });
        }

        public ActionResult UpdateFacilityInfo()
        {

            List<FacilityInfo> appInfo = new List<FacilityInfo>();

            var Updateinfo = (from a in _context.ApplicationRequestForm.AsEnumerable()
                              select new FacilityInfo
                              {
                                  CompanyEmail = a.CompanyEmail,
                                  AgencyName = a.AgencyName,
                                  ApplicationID = a.ApplicationId,
                                  ApplicationTypeId = a.ApplicationTypeId,
                                  AppliedDate = a.AddedDate.ToString(),
                                  CompanyAddress = a.CompanyAddress,
                                  CurrentStageId = Convert.ToInt16(a.CurrentStageId),
                                  LastAssignedUser = a.LastAssignedUser
                              }).ToList();

            appInfo = Updateinfo.GroupBy(x => x.ApplicationID).Select(x => x.LastOrDefault()).ToList();

            ViewBag.AllFacInformation = appInfo;



            return View(appInfo);

        }

        [HttpPost]
        public JsonResult UpdateFacilityRecord(string ApplicationId, string AgencyName, short CurrentStageId, string lastAssignedUser, string ApplicationType, string Companyemail, string CompanyAddress)
        {
            var result = "";

            try
            {
                var appinfo = (from a in _context.ApplicationRequestForm where a.ApplicationId == ApplicationId select a).FirstOrDefault();

                if (appinfo != null)
                {
                    appinfo.CompanyEmail = Companyemail;
                    appinfo.CompanyAddress = CompanyAddress;
                    appinfo.CurrentStageId = CurrentStageId;
                    appinfo.LastAssignedUser = lastAssignedUser;
                    appinfo.AgencyName = AgencyName;
                    appinfo.ApplicationTypeId = ApplicationType;
                    _context.SaveChanges();
                    result = "success";
                }


            }
            catch (Exception ex)
            {
                result = ex.Message;
            }

            return Json(new { Status = result });
        }

        [HttpGet]
        public ActionResult ViewApplication(string applicationId)
        {
            string errorMessage = string.Empty;
            ApplicationRequestForm appRequest = null;
            decimal statutoryFeeAmt = 0, Arrears;

            try
            {
                var paymentlogdetails = (from a in _context.PaymentLog where a.ApplicationId == applicationId select a).FirstOrDefault();
                appRequest = _context.ApplicationRequestForm.Where(c => c.ApplicationId.Trim() == applicationId.Trim()).FirstOrDefault();

                ViewBag.Paid = false;
                Arrears = paymentlogdetails == null ? 0 : paymentlogdetails.Arrears;
                ViewBag.ExtraPayment = (from e in _context.ExtraPayment where e.ApplicationId == appRequest.ApplicationId select e).FirstOrDefault();
                if (ViewBag.ExtraPayment != null) { ViewBag.Amount = ViewBag.ExtraPayment.TxnAmount; ViewBag.PaymentStatus = ViewBag.ExtraPayment.Status; }

                var expirydate = (from a in _context.ApplicationRequestForm where a.CompanyEmail == appRequest.CompanyEmail && a.LicenseReference != null select a.LicenseExpiryDate).ToList().LastOrDefault();

                var ReceivedFrom = (from a in _context.ActionHistory where a.ApplicationId == applicationId select a).ToList().LastOrDefault();
                if (ReceivedFrom != null)
                {
                    ViewBag.ReceivedFrom = ReceivedFrom.TriggeredBy + " (" + ReceivedFrom.TriggeredByRole + ")";
                    ViewBag.LastMessage = ReceivedFrom.Message;
                }
                ViewBag.ReceivedDate = (from a in _context.ApplicationRequestForm where a.ApplicationId == applicationId select a.ModifiedDate).FirstOrDefault();
                var CurrentDesk = appRequest.LastAssignedUser;
                ViewBag.Applicationstatus = appRequest.Status;
                ViewBag.Status = _context.WorkFlowState.Where(w => w.StateId == appRequest.CurrentStageId).FirstOrDefault().StateName;
                var currentdeskname = (from u in _context.UserMaster where u.UserEmail == CurrentDesk select new { u.FirstName, u.LastName }).FirstOrDefault();
                ViewBag.CurrentDesk = CurrentDesk + " (" + currentdeskname.FirstName + " " + currentdeskname.LastName + ")";
                var counthist = _context.ActionHistory.Where(a => a.ApplicationId == applicationId).OrderByDescending(a => a.ActionDate).ToList().Count();
                ViewBag.ActionHistoryList = counthist >= 5 ? _context.ActionHistory.Where(a => a.ApplicationId == applicationId).OrderByDescending(a => a.ActionDate).Take(5) : _context.ActionHistory.Where(a => a.ApplicationId == applicationId).OrderByDescending(a => a.ActionDate).Take(1);
                ViewBag.LoggedInUserRole = _helpersController.getSessionRoleName();

                if (expirydate != null)
                {
                    var ExpiredDate = expirydate.Value.Date;
                    ViewBag.ExpiredDate = ExpiredDate.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
                }
                var TodayDate = DateTime.Now.Date;
                ViewBag.TodayDate = TodayDate.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
                UserMaster applicantMaster = _context.UserMaster.Where(c => c.UserEmail.Trim() == appRequest.CompanyEmail).FirstOrDefault();
                log.Info("About to Get Application Fees");


                if (paymentlogdetails != null)
                {

                    ViewBag.TotalAmount = (Convert.ToDecimal(paymentlogdetails.TxnAmount) + Arrears + statutoryFeeAmt).ToString("N");

                }

                ViewBag.CompanyUploadedDocument = _helpersController.UploadedCompanyDocument(appRequest.ApplicationId);


                if (_helpersController.isPaymentMade(applicationId, out errorMessage))
                {
                    ViewBag.Paid = true;
                }
                ViewBag.ShowPart = false;
                ViewBag.Arrears = Arrears.ToString("N");
                ViewBag.AppRequest = appRequest;
                ViewBag.ApplicationId = appRequest.ApplicationId;
                ViewBag.ErrorMessage = "SUCCESS";
            }
            catch (Exception ex) { ViewBag.ErrorMessage = ex.Message; }

            return View(appRequest);
        }

        [HttpGet]
        public ActionResult ApproveLicense(string applicationId, string myaction, string mycomment)
        {
            ResponseWrapper responseWrapper;
            string response = string.Empty;

            string roleid = _helpersController.getSessionRoleName();
            string comment = string.Empty;
            try
            {
                ApplicationRequestForm appRequest = _context.ApplicationRequestForm.Where(c => c.ApplicationId.Trim() == applicationId).FirstOrDefault();
                if (appRequest ==
                 default(ApplicationRequestForm))
                {
                    return Content("Application ID with Reference " + applicationId + " Cannot be retrievd from the Database");
                }

                if (roleid.Contains("REGISTRAR"))
                {
                    if (myaction == "Accept")
                    {
                        comment = "Certificate Has Been Issued";
                    }
                    else if (myaction == "Reject")
                    {
                        comment = "Your Application has been Rejected";
                    }
                }

                log.Info("Continuing with the Approval to Process Application");
                responseWrapper = _workflowHelper.processAction(appRequest.ApplicationId, myaction, _helpersController.getSessionEmail(), (string.IsNullOrEmpty(mycomment)) ? comment : mycomment);
                if (!responseWrapper.status)
                {
                    response = responseWrapper.value;
                    log.Error(response);
                    return Json(new
                    {
                        status = "failure",
                        Message = response
                    });
                }
            }
            catch (Exception ex)
            {
                log.Error(ex.Message);
                return Json(new { status = "failure", Message = "An Exception occur during Transaction, Please try again Later" });
            }
            GenerateLicenseDocument(applicationId);
            return Json(new { status = "success", Message = responseWrapper.value });
        }

        private void GenerateLicenseDocument(string applicationId)
        {
            try
            {

                _helpersController.GenerateCertificate(applicationId, _helpersController.getSessionEmail());

            }
            catch (Exception ex)
            {
                log.Error(ex.InnerException);
            }

        }

        //public IActionResult ApplicationDetails(string ApplicationId)
        //{
        //    var appdetailvalues = _helpersController.ApplicationDetails(ApplicationId);
        //    return View(appdetailvalues);
        //}


        public ActionResult ViewCertificate(string id)
        {
            var Host = HttpContext.Request.Scheme + "://" + HttpContext.Request.Host + "" + "" + HttpContext.Request.PathBase;

            //return _helpersController.ViewCertificate(id, Host);
            var pdf = _helpersController.ViewCertificate(id, Host);

            return new ViewAsPdf("ViewCertificate", pdf)
            {
                PageSize = (Rotativa.AspNetCore.Options.Size?)Rotativa.Options.Size.A4
            };
        }



    }
}
