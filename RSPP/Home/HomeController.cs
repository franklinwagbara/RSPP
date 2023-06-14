using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Rotativa.AspNetCore;
using RSPP.Configurations;
using RSPP.Models;
using RSPP.Models.DB;
using System;
using System.Linq;

namespace RSPP.Home
{
    public class HomeController : Controller
    {

        public RSPPdbContext _context;
        public IConfiguration _configuration;
        HelperController _helpersController;
        IHttpContextAccessor _httpContextAccessor;

        public HomeController(RSPPdbContext context, IConfiguration configuration, IHttpContextAccessor httpContextAccessor)
        {
            _context = context;
            _configuration = configuration;
            _httpContextAccessor = httpContextAccessor;
            _helpersController = new HelperController(_context, _configuration, _httpContextAccessor);
        }
        public IActionResult Index()
        {
            return View();
        }

        public ActionResult VerifyCertificate()
        {
            var model = new VerifyCertificateModel();
            return View(model);
        }

        /// <summary>
        /// Displays a certificate in the browser, if the certificate license reference number is found
        /// </summary>
        /// <param name="certificate">the verify certificate model</param>
        /// <returns>A pdf or an error response</returns>
        [HttpPost]
        [ValidateAntiForgeryToken]

        public IActionResult ViewCertificate(VerifyCertificateModel certificate)
        {
            if (!ModelState.IsValid)
            {
                return View("VerifyCertificate", certificate);
            }

            certificate.ErrorMessage = "Unable to verify provided certificate id";

            try
            {
                var Host = HttpContext.Request.Scheme + "://" + HttpContext.Request.Host + "" + "" + HttpContext.Request.PathBase;

                var applicationId = (from app in _context.ApplicationRequestForm
                                     where app.LicenseReference == certificate.LicenseReference
                                     select app.ApplicationId).FirstOrDefault();

                if (applicationId != null)
                {

                    var pdf = _helpersController.ViewCertificate(applicationId, Host);

                    if (pdf != null)
                    {
                        certificate.ErrorMessage = "";

                        return new ViewAsPdf("ViewCertificate", pdf)
                        {
                            PageSize = (Rotativa.AspNetCore.Options.Size?)Rotativa.Options.Size.A4
                        };
                    }
                }

            }
            catch (Exception ex)
            {
                throw ex;
            }

            return View("VerifyCertificate", certificate);
        }
    }
}
