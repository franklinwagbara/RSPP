using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RSPP.Helpers;
using RSPP.Models;
using RSPP.Models.DB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RSPP.Controllers
{
    public class VerifyController : Controller
    {

        public RSPPdbContext _context;
        GeneralClass generalClass = new GeneralClass();


        public VerifyController(RSPPdbContext context)
        {
            _context = context;
        }




        [HttpGet]
        [AllowAnonymous]
        public ActionResult VerifyPermitQrCode(string id)
        {
            Permitmodel permit = new Permitmodel();

            if (string.IsNullOrWhiteSpace(id))
            {
                ViewBag.Message = "Something went wrong. Permit not found or not in correct format. Kindly contact support";
            }
            else
            {
               
                    var details = (from a in _context.ApplicationRequestForm
                                   join u in _context.UserMaster on a.CompanyEmail equals u.UserEmail
                                   join f in _context.PaymentLog on a.ApplicationId equals f.ApplicationId
                                   where a.ApplicationId == id
                                   select new { f.TxnAmount, a.CompanyEmail, a.ApplicationId, a.LicenseReference, a.AgencyName, a.PostalAddress, a.LicenseExpiryDate, a.LicenseIssuedDate, u.CompanyName, a.CompanyAddress }).FirstOrDefault();
                    
                    permit.CompanyName = details.CompanyName;
                    permit.LocationAddress = details.PostalAddress;
                    permit.LicenseNumber = details.LicenseReference;
                    permit.RegisteredAddress = details.CompanyAddress;
                    permit.Expiry = Convert.ToDateTime(details.LicenseExpiryDate);
                    permit.AmountToWord = generalClass.NumberToWords(Convert.ToInt64(details.TxnAmount));
                    permit.DateIssued = Convert.ToDateTime(details.LicenseIssuedDate);
                    permit.ApprefNo = details.ApplicationId;
                    permit.Amountpaid = Convert.ToDecimal(details.TxnAmount);

            }
            return View(permit);

        }
    }
}
