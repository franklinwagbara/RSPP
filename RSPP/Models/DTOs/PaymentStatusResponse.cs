namespace RSPP.Models.DTOs
{
    public class PaymentStatusResponse : BasicResponse
    {

        public PaymentStatusResponse() : base(false, null)
        {

        }

        public PaymentStatusResponse(bool success, string message) : base(success, message)
        {
        }

        public GetPaymentResponse PaymentResponse { get; set; }
        public string CompanyName { get; set; }
        public string TransactionAmount { get; set; }
    }
}
