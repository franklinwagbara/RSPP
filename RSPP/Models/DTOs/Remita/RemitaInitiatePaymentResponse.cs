namespace RSPP.Models.DTOs.Remita
{
    public class RemitaInitiatePaymentResponse:BasicResponse
    {
        public RemitaInitiatePaymentResponse() : base(false, null)
        {

        }

        public RemitaInitiatePaymentResponse(bool success, string message) : base(success, message)
        {
        }

        public RemitaInitiatePaymentStatusResponse RemitaInitiatePaymentStatusResponse { get; set; }
    }
}
