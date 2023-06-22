using RSPP.Models.DTOs;

namespace RSPP.Services.Interfaces
{
    public interface IEmailer
    {
        BasicResponse SendEmail(string receiverName, string receiverEmail, string subject, string htmlMessage);
    }
}