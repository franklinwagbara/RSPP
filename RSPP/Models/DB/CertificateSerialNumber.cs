using System;
using System.Collections.Generic;

namespace RSPP.Models.DB
{
    public partial class CertificateSerialNumber
    {
        public int CertificateSerialNumberId { get; set; }
        public long? SerialNumber { get; set; }
    }
}
