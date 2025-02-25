﻿using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using log4net;
using System;
using System.Linq;
using System.Threading.Tasks;
using RSPP.Helpers;
using RSPP.Models.DB;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using RSPP.Configurations;
using Microsoft.AspNetCore.Authentication;
using System.Net.Mail;
using System.Net;
using System.IO;
using Microsoft.AspNetCore.Hosting;
using System.Reflection;
using RSPP.Models.DTOs;
using RSPP.UnitOfWorks.Interfaces;
using RSPP.Services.Interfaces;
using RSPP.Helpers.SerilogService.GeneralLogs;

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
        private readonly string directory = "AdminLogs";
        private readonly GeneralLogger _generalLogger;

        public AccountController(RSPPdbContext context, IHttpContextAccessor httpContextAccessor, IConfiguration configuration, GeneralLogger generalLogger)
        {
            _unitOfWork = unitOfWork;
            _emailer = emailer;
            _context = context;
            _generalLogger = generalLogger;
            _httpContextAccessor = httpContextAccessor;
            _configuration = configuration;
            _helpersController = new HelperController(_context, _configuration, _httpContextAccessor);
        }

        /// <summary>
        /// Displays the login page
        /// </summary>
        [AllowAnonymous]
        public IActionResult Login()
        {
            return View("Login");
        }

        /// <summary>
        /// Authenticates a user
        /// </summary>
        /// <param name="request">AuthenticationRequest request holding email and password</param>
        /// <returns>A basic response</returns>

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public JsonResult Login(AuthenticationRequest request)
        {

            var response = new AuthenticationResponse(false, $"{AppMessages.LOGIN} {AppMessages.FAILED}");
            if (!ModelState.IsValid)
                return Json(response);

            try
            {

                var userMaster = _unitOfWork.UserMasterRepository
                    .Get(u => u.UserEmail.Equals(request.UserEmail) && u.Status.Equals(AppMessages.ACTIVE), null, "", null, null)
                    .FirstOrDefault();
                if (userMaster != null)
                {

                    var authenticationResult = AuthenticateUser(request.UserEmail, request.Password, HttpContext.GetRemoteIPAddress().ToString());

                    if (authenticationResult.Success)
                    {
                        response.Success = true;
                        if (userMaster.EmailConfirmed == false)
                        {
                            response.ResultMessage = $"The registered email <strong>{request.UserEmail}</strong> has not been activated. <br>Kindly check your email for an activation link or click the resend button below";
                            return Json(response);
                        }

                        HttpContext.Session.SetString(sessionEmail, userMaster.UserEmail);
                        HttpContext.Session.SetString(sessionRoleName, userMaster.UserRole);
                        if (userMaster.UserType == AppMessages.COMPANY)
                            HttpContext.Session.SetString(sessionCompanyName, userMaster.CompanyName);
                        else if (userMaster.UserType == AppMessages.ADMIN)
                            HttpContext.Session.SetString(sessionStaffName, userMaster.FirstName.ToString() + " " + userMaster.LastName.ToString());

                        response.IsEmailConfirmed = true;
                        response.UserType = userMaster.UserType;
                        response.ResultMessage = $"{AppMessages.LOGIN} {AppMessages.SUCCESSFUL}";
                        return Json(response);
                    }
                    response.ResultMessage = $"{AppMessages.INVALID_USERNAME_PASSWORD}";
                    return Json(response);
                }
                response.ResultMessage = $"{AppMessages.USER}  {AppMessages.NOT_EXIST}";
                return Json(response);
            }
            catch (Exception ex)
            {
                _logger.Error(ex.StackTrace);
                return Json(response);
            }
        }

        /// <summary>
        /// Displays the registration page
        /// </summary>
        public IActionResult AccountRegister()
        {
            return View("AccountRegister");
        }


        /// <summary>
        /// Registers a user
        /// </summary>
        /// <param name="request">model holding registration details</param>
        /// <returns>A BasicResponse indicating success or failure</returns>

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public JsonResult AccountRegister(RegistrationRequest request)
        {
            var response = new BasicResponse(false, $"{AppMessages.REGISTRATION} {AppMessages.FAILED}");
            if (!ModelState.IsValid)
                return Json(response);

            try
            {
                var userMaster = _unitOfWork.UserMasterRepository
                    .Get(u => u.UserEmail.Equals(request.UserEmail), null, "", null, null).FirstOrDefault();
                if (userMaster != null)
                {
                    response.ResultMessage = $"{AppMessages.EMAIL} {AppMessages.EXISTS}";
                    return Json(response);
                }

                var token = Guid.NewGuid().ToString();
                var newUser = new UserMaster()
                {
                    CompanyName = request.CompanyName,
                    CompanyAddress = request.CompanyAddress,
                    UserEmail = request.UserEmail,
                    PhoneNum = request.MobilePhoneNumber,
                    Password = generalClass.Encrypt(request.Password),
                    UserType = AppMessages.COMPANY,
                    UserRole = AppMessages.COMPANY,
                    //UpdatedBy = request.UserEmail,
                    //LoginCount = 0,
                    CreatedOn = DateTime.Now,
                    Status = AppMessages.ACTIVE,
                    EmailConfirmed = false,
                    EmailConfirmationToken = token
                };

                _unitOfWork.UserMasterRepository.Add(newUser);
                _unitOfWork.Complete();

                response.Success = true;
                response.ResultMessage = $"{AppMessages.REGISTRATION} {AppMessages.SUCCESSFUL}";

                var emailBody = "Please confirm your email address by clicking the following the following link: " + "<a href=\"" + Url.Action("ConfirmEmail", "Account", new { token = token }, protocol: Request.Scheme) + "\">" + Url.Action("ConfirmEmail", "Account", new { token = token }, protocol: Request.Scheme) + "</a>";

                var emailResponse = _emailer.SendEmail(newUser.CompanyName, newUser.UserEmail, $"{AppMessages.EMAIL} {AppMessages.CONFIRMATION}", emailBody);
                if (!emailResponse.Success)
                {
                    _logger.Warn($"{emailResponse.ResultMessage} Email[{newUser.UserEmail}]");

                    response.ResultMessage += $" {AppMessages.EMAIL_VERIFICATION_LINK_FAILED}";
                    return Json(response);
                }

                response.ResultMessage += $". {AppMessages.EMAIL_VERIFICATION_LINK_SENT}";
                return Json(response);
            }
            catch (Exception ex)
            {
                _logger.Error(ex.Message);
                return Json(response);
            }


        }

        [AllowAnonymous]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public JsonResult ResendEmailConfirmation(string Email)
        {
            string responseMessage = string.Empty;
            string status = string.Empty;
            string message = string.Empty;

            try
            {

                _generalLogger.LogRequest($"{"Coming To Login User with Email =>" + Email}{"-"}{DateTime.Now}", false, directory);
                var userMaster = (from u in _context.UserMaster where u.UserEmail == Email select u).FirstOrDefault();
                _generalLogger.LogRequest($"{"Client IpAddress =>" + HttpContext.GetRemoteIPAddress().ToString()}{"-"}{DateTime.Now}", false, directory);
                string validateResult = validateUser(Email, Password, HttpContext.GetRemoteIPAddress().ToString());
                _generalLogger.LogRequest($"{"Validate User Result =>" + validateResult}{"-"}{DateTime.Now}", false, directory);

                if (validateResult == "SUCCESS" && userMaster != null)
                {

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
                _generalLogger.LogRequest($"{"An exception occurred while trying to login"} {ex}{"-"}{DateTime.Now}", false, directory);
                status = "failed";
                message = ex.Message;
                return Json(new { Status = status, Message = message });
            }
        }

        [HttpGet]
        public async Task<IActionResult> LogOff()
        {
            await HttpContext.SignOutAsync();
            return RedirectToAction("Index", "Home");
        }

        #region Helpers

        /// <summary>
        /// Validates a user's credentials
        /// </summary>
        /// <param name="email">user's email</param>
        /// <param name="password">user's password</param>
        /// <param name="ipAddress">the user's ip address</param>
        /// <returns>A basic response</returns>
        private BasicResponse AuthenticateUser(string email, string password, string ipAddress)
        {
            var response = new BasicResponse(false, $"{AppMessages.AUTHENTICATION} {AppMessages.FAILED}");
            try
            {

                password = generalClass.Encrypt(password);
                string responseMessage = string.Empty;
                _generalLogger.LogRequest($"{"Coming To validateUser User =>" + email} {"-"}{DateTime.Now}", false, directory);
                if (string.IsNullOrEmpty(email))
                {
                    return "UserId should Not be Empty";
                }

                _generalLogger.LogRequest($"{"About To Acquire Database Conection for Queries"} {"-"}{DateTime.Now}", false, directory);

                UserMaster userMaster = _context.UserMaster.Where(c => c.UserEmail.Trim() == email.Trim() && c.Password == password).FirstOrDefault();
                _generalLogger.LogRequest($"{"User Details => " + userMaster} {"-"}{DateTime.Now}", false, directory);

                if (userMaster != default(UserMaster))
                {
                    _generalLogger.LogRequest($"{"User Master Status => " + userMaster.Status} {"-"}{DateTime.Now}", false, directory);
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
                    _generalLogger.LogRequest($"{"Saved login session successfully"} {"-"}{DateTime.Now}", false, directory);
                    return "SUCCESS";
                }
                return response;

            }
            catch (Exception ex)
            {
                _generalLogger.LogRequest($"{"User Login -- An Error Occured Validating User"} {ex} {"-"}{DateTime.Now}", false, directory);

                return "An Error Occured Validating User,Please try again Later";
            }
        }

        #endregion


        //[HttpPost]
        //public JsonResult AccountRegister2(string Email, string PhoneNbr, string Companyaddress, string Passwrd, string Companyname)
        //{
        //    /*
        //     * create new usermasterl
        //     * check if email already exists
        //     * **if not-exists populate new user & save , then send email for confirmation
        //     * **if exists, check if user has confirmed email, if not send confirmation email, return response for the email sending
        //     */
        //    string status = string.Empty;
        //    string message = string.Empty;
        //    UserMaster usermaster = new UserMaster();
        //    Passwrd = generalClass.Encrypt(Passwrd);
        //    string token = Guid.NewGuid().ToString();

        //    var checkexistemail = (from u in _context.UserMaster where u.UserEmail == Email select u).FirstOrDefault();
        //    try
        //    {
        //        if (checkexistemail == null)
        //        {
        //            usermaster.CompanyName = Companyname;
        //            usermaster.CompanyAddress = Companyaddress;
        //            usermaster.UserEmail = Email;
        //            usermaster.PhoneNum = PhoneNbr;
        //            usermaster.Password = Passwrd;
        //            usermaster.UserType = "COMPANY";
        //            usermaster.UserRole = "COMPANY";
        //            usermaster.UpdatedBy = Email;
        //            usermaster.LoginCount = 1;
        //            usermaster.LastLogin = DateTime.Now;
        //            usermaster.UpdatedOn = DateTime.Now;
        //            usermaster.CreatedOn = DateTime.Now;
        //            usermaster.Status = "ACTIVE";
        //            usermaster.EmailConfirmed = false;
        //            usermaster.EmailConfirmationToken = token;
        //            _context.Add(usermaster);
        //            _context.SaveChanges();
        //            status = "success";
        //            message = "Your registration was successful";

        //            SendConfirmationEmail(Email, token);

        //        }
        //        else
        //        {
        //            if (usermaster.EmailConfirmed == false)
        //            {
        //                status = "exist";
        //                message = "User with the email: " + Email + " already exist on the portal. A new mail has been sent to confirm your email.";
        //                string sendmail = SendConfirmationEmail(Email, token);
        //                if (sendmail == "failed")
        //                {
        //                    message += "Unable to send Confirmation link to " + Email + ". Please try again later.";

        //                }
        //                else
        //                {
        //                    message += "Confirmation link was successfully sent to " + Email;
        //                }
        //            }
        //            else
        //            {
        //                status = "exist";
        //                message = "User with the email: " + Email + " already exist on the portal. Kindly login with your email";
        //            }

        //            return Json(new { Status = status, Message = message });
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        status = "exist";
        //        message = ex.Message;
        //        return Json(new { Status = status, Message = message });
        //    }

        //    return Json(new { Status = status, Message = message });
        //}

        //private string SendConfirmationEmail(string email, string token)
        //{
        //    string result;
        //    var fromAddress = new MailAddress("rprspu-noreply@nscregistration.gov.ng");
        //    var toAddress = new MailAddress(email);
        //    const string fromPassword = "nsc2018#";
        //    const string subject = "Confirm your email address";
        //    //string callBackURL = 
        //    body = "Please confirm your email address by clicking the following the following link: " + "<a href=\"" + Url.Action("ConfirmEmail", "Account", new { token = token }, protocol: Request.Scheme) + "\">" + Url.Action("ConfirmEmail", "Account", new { token = token }, protocol: Request.Scheme) + "</a>";

        //    var smtp = new SmtpClient
        //    {
        //        Host = "webmail.shipperscouncil.gov.ng", //"smtp.gmail.com",
        //        Port = 587,
        //        EnableSsl = true,
        //        DeliveryMethod = SmtpDeliveryMethod.Network,
        //        Credentials = new NetworkCredential(fromAddress.Address, fromPassword),
        //        Timeout = 20000
        //    };

        //    var message = new MailMessage();
        //    message.From = fromAddress;
        //    message.To.Add(toAddress);
        //    message.Subject = subject;
        //    message.Body = body;
        //    GeneralClass gen = new GeneralClass();
        //    result = "success";
        //    result = gen.ConfirmationEmailMessage(body, toAddress, subject);
        //    return result;
        //}

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
        public JsonResult ForgotPassword(string Email)
        {
            string msg = "";
            var checkifemailexist = (from u in _context.UserMaster where u.UserEmail == Email select u).FirstOrDefault();
            if (checkifemailexist == null)
            {
                msg = "The email " + Email + " does not exist on this portal";
            }
            else
            {
                Random rnd = new Random();
                int value = rnd.Next(100000, 999999);
                string password = "nsc-" + value;
                string content = "Your New Password is " + password;
                string subject = "Forgot Password Activation Link";
                var sendmail = generalClass.ForgotPasswordEmailMessage(Email, subject, content, generalClass.Encrypt(password));
                if (sendmail == "failed")
                {
                    msg = "Unable to send activation link to "+ Email+". Please try again later.";
                    _generalLogger.LogRequest($"{"Unable to send activation link to " + Email + ". Please try again later."} {"-"}{DateTime.Now}", false, directory);

                }
                else
                {
                    msg = "Activation link was successfully sent to " + Email;
                    _generalLogger.LogRequest($"{"Activation link was successfully sent to " + Email} {"-"}{DateTime.Now}", false, directory);

                }
            }
            return Json(new { message = msg });
        }


        public ActionResult PasswordActivation(string Email, string Password)
        {
            var updatepassword = (from u in _context.UserMaster where u.UserEmail == Email select u).FirstOrDefault();
            updatepassword.Password = Password;
            _context.SaveChanges();
            _generalLogger.LogRequest($"{"Passowrd was successfully updated " + Password} {"-"}{DateTime.Now}", false, directory);
            return RedirectToAction("Login");
        }


    }
}
