using System;
using System.Collections.Generic;

namespace RSPP.Models.DB
{
    public partial class UploadedDocuments
    {
        public int DocumentUploadId { get; set; }
        public string DocumentName { get; set; }
        public string DocumentSource { get; set; }
        public string ApplicationId { get; set; }
    }
}
