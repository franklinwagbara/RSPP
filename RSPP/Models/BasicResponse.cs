namespace RSPP.Models
{
    /// <summary>
    /// Base response object for passing messages around the app
    /// </summary>
    public class BasicResponse
    {
        public bool Status { get; set; }
        public string Message { get; set; }
    }
}
