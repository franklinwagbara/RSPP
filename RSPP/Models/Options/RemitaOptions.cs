namespace RSPP.Models.Options
{
    public class RemitaOptions
    {
        public const string Remita = "RemitaTest";
        //public const string Remita = "RemitaLive";
        public string MerchantId { get; set; }
        public string ApiKey { get; set; }
        public string NewServiceId { get; set; }
        public string RenewalServiceId { get; set; }
        public string AccountNumber { get; set; }
        public string BankCode { get; set; }
        public string BaseUrl { get; set; }
        public string TransactionStatusEndpoint { get; set; }
        public string InitiatePaymentEndpoint { get; set; }
    }
}
