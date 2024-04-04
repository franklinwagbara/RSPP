namespace RSPP.Models.DTOs.Remita
{
    public class RemitaTransactionResponse : BasicResponse
    {

        public RemitaTransactionResponse() : base(false, null)
        {

        }

        public RemitaTransactionResponse(bool success, string message) : base(success, message)
        {
        }

        public RemitaTransactionStatusResponse RemitaResponse { get; set; }
        public string TransactionAmount { get; set; }
        public string CompanyName { get; set; }
    }
}
