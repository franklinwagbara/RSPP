namespace RSPP.Models.DTOs
{
    /// <summary>
    /// Base response object for passing messages around the app
    /// </summary>
    public class BasicResponse
    {
        public bool Success { get; set; }
        public string ResultMessage { get; set; } = null;

        public BasicResponse(bool success, string? message)
        {
            Success = success;
            ResultMessage = message;
        }

        public static BasicResponse Result(bool success, string? message) => new BasicResponse(success, message);

    }
}
