using System;
using System.Collections.Generic;

namespace RSPP.Models.DB
{
    public partial class Agency
    {
        public Agency()
        {
            ApplicationRequestForm = new HashSet<ApplicationRequestForm>();
            Documents = new HashSet<Documents>();
        }

        public int AgencyId { get; set; }
        public string AgencyName { get; set; }

        public virtual ICollection<ApplicationRequestForm> ApplicationRequestForm { get; set; }
        public virtual ICollection<Documents> Documents { get; set; }
    }
}
