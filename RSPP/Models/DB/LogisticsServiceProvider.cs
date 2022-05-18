using System;
using System.Collections.Generic;

namespace RSPP.Models.DB
{
    public partial class LogisticsServiceProvider
    {
        public int LogisticsServiceProviderId { get; set; }
        public string LineOfBusiness { get; set; }
        public string CustomLicenseNum { get; set; }
        public string CrffnRegistrationNum { get; set; }
        public string OtherLicense { get; set; }
        public string AnyOtherInfo { get; set; }
        public string ApplicationId { get; set; }
        public DateTime? CustomLicenseExpiryDate { get; set; }
        public DateTime? CrffnRegistratonExpiryDate { get; set; }
        public DateTime? OtherLicenseExpiryDate { get; set; }
    }
}
