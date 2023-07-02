namespace RSPP.Models.DTOs
{
    public class AuthenticationResponse: BasicResponse
    {
        public AuthenticationResponse() : base(false, null)
        {

        }

        public AuthenticationResponse(bool success, string message) : base(success, message)
        {

        }

        public string UserType { get; set; }
        public bool IsEmailConfirmed { get; set; }
    }
}
