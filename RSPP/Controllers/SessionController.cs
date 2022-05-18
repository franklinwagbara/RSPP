using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using RSPP.Configurations;
using RSPP.Helpers;
using RSPP.Models.DB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RSPP.Controllers
{
    public class SessionController : Controller
    {

        private readonly RSPPdbContext _context;
        IHttpContextAccessor _httpContextAccessor;
        public IConfiguration _configuration;
        HelperController _helpersController;
        GeneralClass generalClass = new GeneralClass();


        public SessionController(RSPPdbContext context, IHttpContextAccessor httpContextAccessor, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
            _httpContextAccessor = httpContextAccessor;
            _helpersController = new HelperController(_context, _configuration, _httpContextAccessor);
        }



        [HttpPost]
        public JsonResult CheckSession()
        {
            try
            {
                var session = _helpersController.getSessionEmail();

                string result = "";

                if (session == null || session == "Error" || session == "")
                {
                    result = "true";
                }
                return Json(result);
            }
            catch (Exception)
            {
                return Json("true");
            }
        }


    }
}
