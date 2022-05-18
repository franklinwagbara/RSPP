using System;
using System.Collections.Generic;

namespace RSPP.Models.DB
{
    public partial class UserOfPortService
    {
        public int UserOfPortServiceId { get; set; }
        public string Category { get; set; }
        public string AnyOtherInfo { get; set; }
        public string NepcRegNo { get; set; }
        public string ApplicationId { get; set; }
    }
}
