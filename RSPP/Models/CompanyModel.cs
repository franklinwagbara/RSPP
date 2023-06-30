using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace RSPP.Models
{
    public class ApplicationFormModel
    {

    }


    public class AppResponse
    {
        public AppResponse() { }
        public string message { get; set; }
        public object value { get; set; }
    }


    public class ChangePassword
    {
        public string oldPassword { get; set; }
        public string newPassword { get; set; }
        public string confirmPassword { get; set; }
    }




    public class RRRRequestModel
    {
        public string serviceTypeId { get; set; }
        public string amount { get; set; }
        public string orderId { get; set; }
        public string payerName { get; set; }
        public string payerEmail { get; set; }
        public string payerPhone { get; set; }
        public string description { get; set; }
       
    }

    public class PaymentResponse
    {
        public string statuscode { get; set; }
        public string RRR { get; set; }
        public string status { get; set; }
    }


    public class GetPaymentResponse
    {
        public double amount { get; set; }
        public string RRR { get; set; }
        public string orderId { get; set; }
        public string message { get; set; }
        public string transactiontime { get; set; }
        public string status { get; set; }
    }


    public class WebResponse
    {
        public WebResponse() { }
        public string message { get; set; }
        public object value { get; set; }
    }



    public class AccountModel
    {
        //staff model



        public string CompanyAddress { get; set; }
        public string PhoneNum { get; set; }
        public string CompanyName { get; set; }
        public int? AgencyId { get; set; }
        public string UserEmail { get; set; }
        public string Password { get; set; }
        public string ConfirmPassword { get; set; }
        public string UserType { get; set; }
        public string UserRole { get; set; }
        public DateTime? CreatedOn { get; set; }
        public string UpdatedBy { get; set; }
        public DateTime? UpdatedOn { get; set; }
        public string Status { get; set; }
        public DateTime? LastLogin { get; set; }
        public int? LoginCount { get; set; }
        public string LastComment { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }

    }




    public class DocUpload
    {
        public int DocId { get; set; }
        public string DocumentName { get; set; }
        public string UploadedDocName { get; set; }
        public string IsMandatory { get; set; }
        public string DocumentSource { get; set; }
        public string ApplicationId { get; set; }
        public IList<IFormFile> files { get; set; }
    }



    public class PasswordModel
    {
        public string OldPassword { get; set; }
        public string NewPassword { get; set; }
        public string ConfirmPassword { get; set; }
    }



    public class GeneralCommentModel
    {
        public string Comment { get; set; }
        public string ApplicationID { get; set; }
    }





    public class CompanyMessage
    {
        public string ApplicationId { get; set; }
        public string MessageId { get; set; }
        public string Message { get; set; }
        public string MessageType { get; set; }
        public string Subject { get; set; }
        public String Date { get; set; }
    }

}
