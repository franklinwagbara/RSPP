using System;
using System.Collections.Generic;

namespace RSPP.Models.DB
{
    public partial class GovernmentAgency
    {
        public int GovAgencyId { get; set; }
        public string ServicesProvidedInPort { get; set; }
        public string AnyOtherRelevantInfo { get; set; }
        public string ApplicationId { get; set; }
    }
}
