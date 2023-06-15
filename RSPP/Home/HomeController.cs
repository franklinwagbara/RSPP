using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Rotativa.AspNetCore;
using RSPP.Configurations;
using RSPP.Models;
using RSPP.Models.DB;
using System;
using System.Diagnostics;
using System.Linq;

namespace RSPP.Home
{
    public class HomeController : Controller
    {

        public RSPPdbContext _context;
        public IConfiguration _configuration;
        HelperController _helpersController;
        IHttpContextAccessor _httpContextAccessor;
        private readonly ILogger<HomeController> _logger;

        public HomeController(
            RSPPdbContext context, 
            IConfiguration configuration, 
            IHttpContextAccessor httpContextAccessor,
            ILogger<HomeController> logger)
        {
            _logger = logger;
            _context = context;
            _configuration = configuration;
            _httpContextAccessor = httpContextAccessor;
            _helpersController = new HelperController(_context, _configuration, _httpContextAccessor);
        }
        public IActionResult Index()
        {
            //_logger.LogInformation("Index() called");
            //var rng = new Random();
            //_logger.LogWarning("Random object was created");
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

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {

            var data = HttpContext.Request.Headers["4o4Path"];
            _logger.LogError(data);

            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
