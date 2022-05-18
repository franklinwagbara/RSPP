using System;
using System.Collections.Generic;

namespace RSPP.Models.DB
{
    public partial class PaymentCategory
    {
        public int PaymentCategoryId { get; set; }
        public string PaymentCategoryName { get; set; }
        public decimal? PaymentAmount { get; set; }
    }
}
