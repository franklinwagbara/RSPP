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
using System.Net.Mail;
using System.Web;
using System.Net;
using System.IO;
using Microsoft.AspNetCore.Hosting;

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
        public string body;
        public const string sessionEmail = "_sessionEmail";
        public const string sessionStaffName = "_sessionStaffName";
        public const string sessionRoleName = "_sessionRoleName";
        public const string sessionCompanyName = "_sessionCompanyName";
        private static readonly log4net.ILog Logger = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        [Obsolete]
        private readonly IHostingEnvironment _hostingEnv;


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
        public IActionResult TestAlert()
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

                    if (userMaster.UserType.Contains("COMPANY") && userMaster.EmailConfirmed == true)
                    {

                        status = "success";
                        message = "Company";
                        HttpContext.Session.SetString(sessionCompanyName, userMaster.CompanyName);

                        return Json(new { Status = status, Message = message });
                    }
                    else if (userMaster.UserType.Contains("COMPANY") && userMaster.EmailConfirmed == false)
                    {
                        status = "failed";
                        //var token = userMaster.EmailConfirmationToken;
                        //SendConfirmationEmail(Email, token);
                        return Json(new { Status = status, Message = "The registered email : " + Email+ " has not been activated. Kindly check your email for an activation link or click the resend button below" });
                        //return Json(new { Status = status, Message = "User with this email: " + Email+ " is registered on the portal. A mail has been sent to you to confirm your email address!!!"});
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
                    return Json(new { Status = status, Message = message }); //Content("<html><head><script>alert(\"" + validateResult + "\");window.location.replace('LogOff')</script></head></html>");
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



                if (userMaster != default(UserMaster))
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
        private string SendConfirmationEmail(string email, string token)
        {
            string result;
            var fromAddress = new MailAddress("rprspu-noreply@nscregistration.gov.ng"); //new MailAddress("quadrymustaqeem@gmail.com","Mustaqeem Quadry");
            var toAddress = new MailAddress(email);
            const string fromPassword = "nsc2018#";
            const string subject = "Confirm your email address";
            //string callBackURL = 
            body = "Please confirm your email address by clicking the following the following link: " + "<a href=\"" + Url.Action("ConfirmEmail", "Account", new { token = token }, protocol: Request.Scheme) + "\">" + Url.Action("ConfirmEmail", "Account", new { token = token }, protocol: Request.Scheme) + "</a>";

            var smtp = new SmtpClient
            {
                Host = "webmail.shipperscouncil.gov.ng", //"smtp.gmail.com",
                Port = 587,
                EnableSsl = true,
                DeliveryMethod = SmtpDeliveryMethod.Network,
                Credentials = new NetworkCredential(fromAddress.Address, fromPassword),
                Timeout = 20000
            };

            var message = new MailMessage();
            message.From = fromAddress;
            message.To.Add(toAddress);
            message.Subject = subject;
            message.Body = body;
            GeneralClass gen = new GeneralClass();
            result = gen.ConfirmationEmailMessage(body, toAddress, subject);
            //smtp.Send(message);
            return result;
        }

        public ActionResult ConfirmEmail(string token)
        {
            var user = (from u in _context.UserMaster where u.EmailConfirmationToken == token select u).FirstOrDefault();



            if (user != null)
            {
                // Update the user's account to confirm their email address
                user.EmailConfirmed = true;
                user.EmailConfirmationToken = null;
                _context.SaveChanges();

                // Redirect the user to a "confirmation success" page
                return RedirectToAction("ConfirmationSuccess");
            }
            else
            {
                // Redirect the user to a "confirmation error" page
                return RedirectToAction("ConfirmationError");
            }
        }
        public ActionResult ConfirmationSuccess()
        {
            return RedirectToAction("Login");
        }

        // GET: Account/ConfirmationError
        public ActionResult ConfirmationError()
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
            string token = Guid.NewGuid().ToString();
            
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
                    usermaster.EmailConfirmed = false;
                    usermaster.EmailConfirmationToken = token;
                    _context.Add(usermaster);
                    _context.SaveChanges();
                    status = "success";
                    message = "Your registration was successful";

                    SendConfirmationEmail(Email, token);

                }
                else
                {
                    if (usermaster.EmailConfirmed == false)
                    {
                        status = "exist";
                        message = "User with the email: " + Email + " already exist on the portal. A new mail has been sent to confirm your email.";
                        string sendmail = SendConfirmationEmail(Email, token);
                        if (sendmail == "failed")
                        {
                            message += "Unable to send Confirmation link to " + Email + ". Please try again later.";

                        }
                        else
                        {
                            message += "Confirmation link was successfully sent to " + Email;
                        }
                    }
                    else
                    {
                        status = "exist";
                        message = "User with the email: " + Email + " already exist on the portal. Kindly login with your email";
                    }
                    
                    return Json(new { Status = status, Message = message });
                }
            }
            catch(Exception ex)
            {
                status = "exist";
                message = ex.Message;
                return Json(new { Status = status, Message = message });
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
            StreamWriter sw = new StreamWriter("log.txt");
            sw.WriteLine(Password);
            sw.Close();
            if (Password != null)
            {
                var updatepassword = (from u in _context.UserMaster where u.UserEmail == Email select u).FirstOrDefault();
                updatepassword.Password = Password;
                _context.SaveChanges();
                return RedirectToAction("Login");
            }
            else
            {
                return RedirectToAction("ConfirmationError");
            }
        }


    }
}
