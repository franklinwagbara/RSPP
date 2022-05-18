using System;
using System.Collections.Generic;

namespace RSPP.Models.DB
{
    public partial class UserMaster
    {
        public int UserMasterId { get; set; }
        public string UserEmail { get; set; }
        public string CompanyAddress { get; set; }
        public string Password { get; set; }
        public string PhoneNum { get; set; }
        public string CompanyName { get; set; }
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
        public string SignatureImage { get; set; }
    }
}
