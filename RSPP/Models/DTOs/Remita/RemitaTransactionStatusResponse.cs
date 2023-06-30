namespace RSPP.Models.DTOs.Remita
{
    public class RemitaTransactionStatusResponse
    {
        public string amount { get; set; }
        public string RRR { get; set; }
        public string orderId { get; set; }
        public string message { get; set; }
        public string transactiontime { get; set; }
        public string status { get; set; }
    }
}
