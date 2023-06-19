using log4net;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Rotativa.AspNetCore;
using RSPP.Configurations;
using RSPP.Models;
using RSPP.Models.DB;
using System;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;

namespace RSPP.Home
{
    public class HomeController : Controller
    {

        public RSPPdbContext _context;
        public IConfiguration _configuration;
        HelperController _helpersController;
        IHttpContextAccessor _httpContextAccessor;
        private static readonly ILog _logger = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);


        public HomeController(
            RSPPdbContext context,
            IConfiguration configuration,
            IHttpContextAccessor httpContextAccessor
            )
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
        /// Matches the following patterns
        /// NSC/RRPSPU/029/2023
        /// nsc/rrpspu/023/2022
        /// nsc/rprspu/022/2021
        /// NSC/RPRSPU/022/2021
        /// Nsc/Rprspu/023/2022
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

            string pattern = @"^(NSC\/((RRPSPU)|(RPRSPU))\/)\d{3}\/\d{4}$";
            if (!Regex.IsMatch(certificate.LicenseReference, pattern, RegexOptions.IgnoreCase))
                return View("VerifyCertificate", certificate);

            var selectedCertificateReference = certificate.LicenseReference.ToUpper();

            try
            {
                var Host = HttpContext.Request.Scheme + "://" + HttpContext.Request.Host + "" + "" + HttpContext.Request.PathBase;

                var applicationId = (from app in _context.ApplicationRequestForm
                                     where app.LicenseReference == selectedCertificateReference
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
                _logger.Error(ex);
            }

            return View("VerifyCertificate", certificate);
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]

        public IActionResult Error404()
        {

            var data = HttpContext.Request.Headers["4o4Path"];
            _logger.Error(data);

            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
