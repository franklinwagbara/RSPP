using System;
using System.Collections.Generic;

namespace RSPP.Models.DB
{
    public partial class PaymentLog
    {
        public long PaymentId { get; set; }
        public string ApplicationId { get; set; }
        public string ApplicantId { get; set; }
        public DateTime? TransactionDate { get; set; }
        public string TransactionId { get; set; }
        public string PaymentCategory { get; set; }
        public string Description { get; set; }
        public string Rrreference { get; set; }
        public string AppReceiptId { get; set; }
        public decimal? TxnAmount { get; set; }
        public decimal Arrears { get; set; }
        public string BankCode { get; set; }
        public string Account { get; set; }
        public string TxnMessage { get; set; }
        public string Status { get; set; }
        public int? RetryCount { get; set; }
        public DateTime? LastRetryDate { get; set; }
        public string ActionBy { get; set; }
    }
}
