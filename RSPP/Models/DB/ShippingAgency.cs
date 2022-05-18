using System;
using System.Collections.Generic;

namespace RSPP.Models.DB
{
    public partial class ShippingAgency
    {
        public int ShippingAgencyId { get; set; }
        public string LineOfBusiness { get; set; }
        public string VesselLinesRepresentedInNigeria { get; set; }
        public string CargoType { get; set; }
        public string AnyOtherInfo { get; set; }
        public string Nparegnum { get; set; }
        public string Nimasaregnum { get; set; }
        public string ApplicationId { get; set; }
    }
}
