using System;
using System.Collections.Generic;

namespace RSPP.Models.DB
{
    public partial class OutofOffice
    {
        public int OutofOfficeId { get; set; }
        public string Reliever { get; set; }
        public string Relieved { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public string Comment { get; set; }
        public string Status { get; set; }
    }
}
