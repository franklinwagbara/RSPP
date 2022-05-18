using System;
using System.Collections.Generic;

namespace RSPP.Models.DB
{
    public partial class Documents
    {
        public int DocId { get; set; }
        public string DocumentName { get; set; }
        public int? LineOfBusinessId { get; set; }
        public string IsMandatory { get; set; }

        public virtual Agency LineOfBusiness { get; set; }
    }
}
