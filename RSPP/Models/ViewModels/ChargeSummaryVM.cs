namespace RSPP.Models.ViewModels
{
    public class ChargeSummaryVM
    {
        public string ApplicationId { get; set; }
        public string ApiKey { get; set; }
        public string MerchantId { get; set; }
        public string RRR { get; set; }
        public string AgencyName { get; set; }
        public string Amount { get; set; }
        public string BaseUrl { get; set; }
        public string FinalizePaymentURL { get; set; }
        public string ErrorMessage { get; set; }
    }
}
