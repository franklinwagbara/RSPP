using System;
using System.Collections.Generic;

namespace RSPP.Models.DB
{
    public partial class MissingDocuments
    {
        public int MissingDocId { get; set; }
        public string ApplicationId { get; set; }
        public int DocId { get; set; }

        public virtual ApplicationRequestForm Application { get; set; }
    }
}
