using System;
using System.Collections.Generic;

namespace RSPP.Models.DB
{
    public partial class OtherPortServiceProvider
    {
        public int OtherPortServiceProviderId { get; set; }
        public string LineOfBusiness { get; set; }
        public string AnyOtherInfo { get; set; }
        public string ApplicationId { get; set; }
    }
}
