using RSPP.Helpers;

namespace RSPP.Models.Options
{
    public class RemitaOptions
    {
        public const string Remita = AppMessages.REMITA_TEST;
        //public const string Remita = AppMessages.REMITA_LIVE;
        public string MerchantId { get; set; }
        public string ApiKey { get; set; }
        public string NewServiceId { get; set; }
        public string RenewalServiceId { get; set; }
        public string AccountNumber { get; set; }
        public string BankCode { get; set; }
        public string BaseUrl { get; set; }
        public string PortalBaseUrl { get; set; }
        public string TransactionStatusEndpoint { get; set; }
        public string InitiatePaymentEndpoint { get; set; }
        public string FinalizePaymentURL { get; set; }
    }
}
