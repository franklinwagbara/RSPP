using RSPP.Models;

namespace RSPP.Models.ViewModels
{
    public class PaymentReceiptVM
    {
        public string ApplicationId { get; set; }
        public string PaymentStatus { get; set; }
        public string RRR { get; set; }
        public string TransactionDate { get; set; }
        public string TransactionMessage { get; set; }
        public string TotalAmount { get; set; }
    }
}
