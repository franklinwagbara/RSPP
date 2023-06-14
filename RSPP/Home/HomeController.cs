using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Rotativa.AspNetCore;
using RSPP.Configurations;
using RSPP.Models;
using RSPP.Models.DB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

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
            return View();
        }

        /// <summary>
        /// Displays a certificate in the browser, if the permit number is found
        /// </summary>
        /// <param name="id">the permit number</param>
        /// <returns>A pdf or an error response</returns>
        [HttpPost]
        [ValidateAntiForgeryToken]

        public IActionResult ViewCertificate(VerifyCertificateModel certificate)
        {
            if (!ModelState.IsValid)
            {
                return View("VerifyCertificate", certificate);
            }

            if (!certificate.CertificateId.All(Char.IsDigit))
                return View("VerifyCertificate", certificate);

            try
            {

                var Host = HttpContext.Request.Scheme + "://" + HttpContext.Request.Host + "" + "" + HttpContext.Request.PathBase;

                var pdf = _helpersController.ViewCertificate(certificate.CertificateId.ToString(), Host);

                if (pdf != null)
                {

                    return new ViewAsPdf("ViewCertificate", pdf)
                    {
                        PageSize = (Rotativa.AspNetCore.Options.Size?)Rotativa.Options.Size.A4
                    };
                }

            }
            catch (Exception ex)
            {
                throw ex;
            }
            return View("Home");
        }
    }
}
