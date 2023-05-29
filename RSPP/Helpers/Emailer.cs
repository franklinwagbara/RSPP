using MimeKit;
using MailKit.Security;
using MailKit.Net.Smtp;

namespace RSPP.Helpers
{
    public class Emailer
    {

        private const string SENDER_NAME = "Shippers Council";
        private const string SENDER_EMAIL_ADDRESS = "ch9labsbybincom@gmail.com";
        private const string SMTP_HOST = "smtp.gmail.com";
        private const int SMTP_PORT = 587;
        public void SendEmail(string receiverName, string receiverEmail, string subject, string htmlMessage)
        {

            var email = new MimeMessage();
            email.From.Add(new MailboxAddress(SENDER_NAME, SENDER_EMAIL_ADDRESS));
            email.To.Add(new MailboxAddress(receiverName, receiverEmail));
            email.Subject = subject;
            email.Body = new TextPart(MimeKit.Text.TextFormat.Html)
            {
                Text = htmlMessage
            };

            using (var smtp = new SmtpClient())
            {
                var smtpUsername = SENDER_EMAIL_ADDRESS;
                //var smtpUsername = (!string.IsNullOrWhiteSpace(Environment.GetEnvironmentVariable("SMTP_USERNAME")))
                //? Environment.GetEnvironmentVariable("SMTP_USERNAME") : _configuration["SMTP_USERNAME"];

                var smtpPassword = "ecjmjbvtkuwgucic";
                //var smtpPassword = (!string.IsNullOrWhiteSpace(Environment.GetEnvironmentVariable("SMTP_PASSWORD")))
                //? Environment.GetEnvironmentVariable("SMTP_PASSWORD") : _configuration["SMTP_PASSWORD"];

                smtp.Connect(SMTP_HOST, SMTP_PORT, SecureSocketOptions.StartTls);

                smtp.Authenticate(smtpUsername, smtpPassword);
                smtp.Send(email);
                smtp.Disconnect(true);
            }

        }
    }
}
