using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using log4net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using log4net.Repository;
using RSPP.Helpers;
using RSPP.Models.DB;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using RSPP.Configurations;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using System.Security.Claims;

namespace RSPP.Controllers
{
    public class AccountController : Controller
    {
        public IConfiguration _configuration;
        private readonly RSPPdbContext _context;
        Authentications auth = new Authentications();
        IHttpContextAccessor _httpContextAccessor;
        HelperController _helpersController;
        GeneralClass generalClass = new GeneralClass();
        public static string roleid;
        public const string sessionEmail = "_sessionEmail";
        public const string sessionStaffName = "_sessionStaffName";
        public const string sessionRoleName = "_sessionRoleName";
        public const string sessionCompanyName = "_sessionCompanyName";
        private static readonly log4net.ILog Logger = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);


        public AccountController(RSPPdbContext context, IHttpContextAccessor httpContextAccessor, IConfiguration configuration)
        {
            _context = context;
            _httpContextAccessor = httpContextAccessor;
            _configuration = configuration;
            _helpersController = new HelperController(_context, _configuration, _httpContextAccessor);
        }


        [AllowAnonymous]
        public IActionResult Login()
        {
            return View();
        }

        [AllowAnonymous]
        [HttpPost]
        public JsonResult Login(string Email, string Password)
        {
            string responseMessage = string.Empty;
            string status = string.Empty;
            string message = string.Empty;
            try
            {


                Logger.Info("Coming To Login User with Email =>" + Email);

                var userMaster = (from u in _context.UserMaster where u.UserEmail == Email select u).FirstOrDefault();

                Logger.Info("Client IpAddress =>" + HttpContext.GetRemoteIPAddress().ToString());
                string validateResult = validateUser(Email, Password, HttpContext.GetRemoteIPAddress().ToString());

                Logger.Info("Validate User Result =>" + validateResult);

                if (validateResult == "SUCCESS" && userMaster != null)
                {

                    //var identity = new ClaimsIdentity(new[]{new Claim(ClaimTypes.Role, userMaster.UserRole),}, CookieAuthenticationDefaults.AuthenticationScheme);
                    //var principal = new ClaimsPrincipal(identity);
                    //var getin = HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal);


                    HttpContext.Session.SetString(sessionEmail, userMaster.UserEmail);
                    HttpContext.Session.SetString(sessionRoleName, userMaster.UserRole);

                    if (userMaster.UserType.Contains("COMPANY"))
                    {
                        status = "success";
                        message = "Company";
                        HttpContext.Session.SetString(sessionCompanyName, userMaster.CompanyName);

                        return Json(new{Status = status, Message = message });
                    }
                    else
                    {
                        HttpContext.Session.SetString(sessionStaffName, userMaster.FirstName.ToString() + " " + userMaster.LastName.ToString());

                        status = "success";
                        message = "Admin";
                        return Json(new { Status = status, Message = message });
                    }

                    

                }
                else
                {
                    status = "failed";
                    message = "Login failed!! please check your login credentials";
                    return Json(new {Status = status, Message = message }); //Content("<html><head><script>alert(\"" + validateResult + "\");window.location.replace('LogOff')</script></head></html>");
                }


            }
            catch (Exception ex)
            {
                Logger.Error(ex.StackTrace);
                status = "failed";
                message = ex.Message;
                return Json(new { Status = status, Message = message });//Content("<html><head><script>alert(\"" + ex.Message + "\");window.location.replace('LogOff')</script></head></html>");
            }
        }


        [HttpGet]
        public async Task<IActionResult> LogOff()
        {
            await HttpContext.SignOutAsync();
            return RedirectToAction("Index", "Home");
        }

        //[HttpPost]
        //public ActionResult LogOff()
        //{
        //    var elpsLogOffUrl = Request.PathBase + "/Account/RemoteLogOff";
        //    var returnUrl = Url.Action("Index", "Home", null, Request.Scheme);
        //    //var returnUrl = Request.Url.GetLeftPart(UriPartial.Authority) + Request.ApplicationPath;
        //    var frm = "<form action='" + elpsLogOffUrl + "' id='frmTest' method='post'>" + "<input type='hidden' name='returnUrl' value='" + returnUrl + "' />"+ "</form>" + "<script>document.getElementById('frmTest').submit();</script>";
        //    return Content(frm, "text/html");
        //}

        #region Helpers
        private string validateUser(string email, string password, string ipAddress)
        {

            try
            {
                password = generalClass.Encrypt(password);
                string responseMessage = string.Empty;
                Logger.Info("Coming To validateUser User =>" + email);
                if (string.IsNullOrEmpty(email))
                {
                    return "UserId should Not be Empty";
                }

                Logger.Info("About To Acquire Database Conection for Queries");

                Logger.Info("About To Retrieve UserMaster Details from the System");
                UserMaster userMaster = _context.UserMaster.Where(c => c.UserEmail.Trim() == email.Trim() && c.Password == password).FirstOrDefault();
                Logger.Info("User Details => " + userMaster);



                if (userMaster !=default(UserMaster))
                {
                    Logger.Info("User Master Status => " + userMaster.Status);
                    UserLogin userLogin = new UserLogin();
                    userLogin.UserEmail = userMaster.UserEmail;
                    userLogin.UserType = userMaster.UserType;
                    userLogin.Browser = auth.BrowserName(HttpContext);
                    userLogin.Client = ipAddress;
                    userLogin.LoginMessage = (userMaster.Status == "ACTIVE") ? "LoggedIN" : userMaster.FirstName + "(" + email + ") is not Active on the platform";
                    userLogin.LoginTime = DateTime.Now;
                    userLogin.Status = userMaster.Status;
                    _context.UserLogin.Add(userLogin);
                    _context.SaveChanges();

                    Logger.Info("About To Maintain User on Session");

                    Logger.Info("Done With Session");

                    return "SUCCESS";
                }
                else
                {
                    return responseMessage;
                }

                //}

            }
            catch (Exception ex)
            {
                Logger.Error(ex.StackTrace);
                return "An Error Occured Validating User,Please try again Later";
            }
        }

        #endregion
        public IActionResult AccountRegister()
        {
            return View();
        }

        [HttpPost]
        public JsonResult AccountRegister(string Email, string PhoneNbr, string Companyaddress, string Passwrd, string Companyname)
        {
            string status = string.Empty;
            string message = string.Empty;
            UserMaster usermaster = new UserMaster();
            Passwrd = generalClass.Encrypt(Passwrd);
            var checkexistemail = (from u in _context.UserMaster where u.UserEmail == Email select u).FirstOrDefault();
            try
            {
                if (checkexistemail == null)
                {
                    usermaster.CompanyName = Companyname;
                    usermaster.CompanyAddress = Companyaddress;
                    usermaster.UserEmail = Email;
                    usermaster.PhoneNum = PhoneNbr;
                    usermaster.Password = Passwrd;
                    usermaster.UserType = "COMPANY";
                    usermaster.UserRole = "COMPANY";
                    usermaster.UpdatedBy = Email;
                    usermaster.LoginCount = 1;
                    usermaster.LastLogin = DateTime.Now;
                    usermaster.UpdatedOn = DateTime.Now;
                    usermaster.CreatedOn = DateTime.Now;
                    usermaster.Status = "ACTIVE";
                    _context.Add(usermaster);
                    _context.SaveChanges();
                    status = "success";
                    message = "Your registration was successful";
                }
                else
                {
                    status = "exist";
                    message = "Record with the email "+Email+" already exist in the database.";
                }
            }
            catch(Exception ex)
            {
                status = "failed";
                message = ex.Message;
                //return Json(new { Status = status, Message = message });
            }

            return Json(new { Status = status, Message = message });
        }


        [HttpPost]
        public JsonResult ForgotPassword(string Email)
        {
            string msg = "";
            var checkifemailexist = (from u in _context.UserMaster where u.UserEmail == Email select u).FirstOrDefault();
            if(checkifemailexist == null)
            {
                msg = "The email " + Email + " does not exist on this portal";
            }
            else
            {
                Random rnd = new Random();
                int value = rnd.Next(100000, 999999);
                string password = "nsc-" + value;
                string content = "Your New Password is "+ password;
                string subject = "Forgot Password Activation Link";
                var sendmail = generalClass.ForgotPasswordEmailMessage(Email, subject, content, generalClass.Encrypt(password));
                 if(sendmail == "failed")
                {
                    msg = "Unable to send activation link to "+ Email+". Please try again later.";
                }
                else {
                    msg = "Activation link was successfully sent to " + Email;
                }
            }
            return Json(new {message = msg });
        }


        public ActionResult PasswordActivation(string Email, string Password)
        {
            var updatepassword = (from u in _context.UserMaster where u.UserEmail == Email select u).FirstOrDefault();
            updatepassword.Password = Password;
            _context.SaveChanges();
            return RedirectToAction("Login");
        }


    }
}
