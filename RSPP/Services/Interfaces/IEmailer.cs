using RSPP.Models.DTOs;

namespace RSPP.Services.Interfaces
{
    /// <summary>
    /// Provides interface for sending emails
    /// </summary>
    public interface IEmailer
    {
        /// <summary>
        /// Provides interface for sending emails
        /// </summary>
        /// <param name="receiverName">Name of receiver</param>
        /// <param name="receiverEmail">Name of receiver</param>
        /// <param name="subject">Name of receiver</param>
        /// <param name="htmlMessage">Name of receiver</param>
        /// <returns>An object representing success or failure of the action</returns>
        BasicResponse SendEmail(string receiverName, string receiverEmail, string subject, string htmlMessage);
    }
}