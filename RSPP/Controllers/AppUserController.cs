using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using RSPP.Helpers;
using RSPP.Models;
using RSPP.Models.DB;
using RSPP.Services.Interfaces;
using System.Collections.Generic;
using System.IO;

namespace RSPP.Controllers
{
    [SessionTimeout]
    /// <summary>
    /// Base Controller for features common to admin & company
    /// I may modify this later depending on the other features in the app
    /// </summary>
    public class AppUserController : Controller
    {
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly IPaymentService _paymentService;

        private const string PDF_MIME_TYPE = "application/pdf";
        protected const string VIDEO_GUIDES_COMPANY_PATH = "UserGuides/Video/";
        protected const string USER_GUIDES_COMPANY_PATH = "UserGuides/Company/";
        protected const string USER_GUIDES_ADMIN_PATH = "UserGuides/Admin/";
        protected const string USER_TYPE_COMPANY = "COMPANY";
        protected const string USER_TYPE_ADMIN = "ADMIN";

        public List<FileModel> Files { get; set; } = new List<FileModel>();
        public AppUserController(IWebHostEnvironment webHostEnvironment,IPaymentService paymentService)
        {
            _webHostEnvironment = webHostEnvironment;
            _paymentService = paymentService;
        }

        /// <summary>
        /// Gets the user guide files for a user
        /// </summary>
        /// <param name="userType">company or admin(officer,supervisor,registrar) user</param>
        /// <param name="path">path to file</param>
        /// <returns>A list of file names</returns>
        public List<FileModel> GetUserGuides(string userType, string path)
        {
            Files = new List<FileModel>();
            var filePaths = Directory.GetFiles(Path.Combine(_webHostEnvironment.WebRootPath, path));
            foreach (var file in filePaths)
            {
                Files.Add(new FileModel { UserType = userType, FileName = Path.GetFileName(file) });
            }
            return Files;
        }

        /// <summary>
        /// View a file in the browser
        /// </summary>
        /// <param name="userType">company or admin(officer,supervisor,registrar) user</param>
        /// <param name="fileName">name of file</param>
        /// <returns>A file result</returns>
        [HttpGet]
        public ActionResult ViewFile(string userType, string fileName)
        {
            var path = userType == USER_TYPE_ADMIN ? USER_GUIDES_ADMIN_PATH : USER_GUIDES_COMPANY_PATH;
            string filePath = Path.Combine(_webHostEnvironment.WebRootPath, path) + fileName;

            byte[] bytes = System.IO.File.ReadAllBytes(filePath);
            MemoryStream stream = new MemoryStream(bytes);

            return File(stream, PDF_MIME_TYPE);
        }

        /// <summary>
        /// Downloads a file to the user's computer
        /// </summary>
        /// <param name="userType">company or admin(officer,supervisor,registrar) user</param>
        /// <param name="fileName">name of file</param>
        /// <returns>A file stream result</returns>
        public ActionResult DownloadFile(string userType, string fileName)
        {
            var path = userType == USER_TYPE_ADMIN ? USER_GUIDES_ADMIN_PATH : USER_GUIDES_COMPANY_PATH;
            string filePath = Path.Combine(_webHostEnvironment.WebRootPath, path) + fileName;

            byte[] bytes = System.IO.File.ReadAllBytes(filePath);
            MemoryStream stream = new MemoryStream(bytes);

            return new FileStreamResult(stream, PDF_MIME_TYPE)
            {
                FileDownloadName = fileName
            };

        }

        public string VideoUserGuide()
        {
            return "~/" + VIDEO_GUIDES_COMPANY_PATH + "user_guide.mp4";
        }

        /// <summary>
        /// Gets the status for payment attached to an application
        /// </summary>
        /// <param name="applicationId">application id</param>
        /// <returns>A JsonResult representing payment status for that application</returns>
        [HttpPost]
        [ValidateAntiForgeryToken]

        public JsonResult CheckPaymentStatus(string applicationId)
        {
            return Json(_paymentService.CheckPaymentStatusAsync(applicationId));
        }
    }
}
