using System;
using System.Collections.Generic;

namespace RSPP.Models.DB
{
    public partial class LineOfBusiness
    {
        public int LineOfBusinessId { get; set; }
        public string LineOfBusinessName { get; set; }
        public decimal Amount { get; set; }
        public int? FormTypeId { get; set; }
        public int OrderId { get; set; }
    }
}
