﻿using MimeKit;
using MailKit.Security;
using MailKit.Net.Smtp;
using System.Text.RegularExpressions;
using System;
using Microsoft.AspNetCore.Mvc;
using RSPP.Models;
using log4net.Repository.Hierarchy;

namespace RSPP.Helpers
{

    /// <summary>
    /// Class for verifying & sending emails
    /// </summary>
    public static class Emailer
    {

        private const string SENDER_NAME = "Nigerian Shippers Council";
        private const string SENDER_EMAIL_ADDRESS = "rprspu-noreply@nscregistration.gov.ng";
        private const string SMTP_HOST = "webmail.shipperscouncil.gov.ng";
        private const string MAIL_PASSWORD = "nsc2018#"; // remove this later
        private const int SMTP_PORT = 587;
        private const string MSG_INVALID_PARAMERTERS= "Invalid email parameters provided";
        private const string MSG_INVALID_EMAIL_FORMAT= "Email has invalid format";
        private const string MSG_MESSAGE_SENDING_SUCCESSFUL= "Email sent successfully";
        private const string MSG_MESSAGE_SENDING_FAILED= "Email sent failed";


        /// <summary>
        /// Sends an email
        /// </summary>
        /// <param name="receiverName">receiver's name</param>
        /// <param name="receiverEmail">receiver's email</param>
        /// <param name="subject">subject of the email</param>
        /// <param name="htmlMessage">body of the email</param>
        /// <returns>A basic response</returns>
        public static BasicResponse SendEmail(string receiverName, string receiverEmail, string subject, string htmlMessage)
        {
            var response = new BasicResponse { Status = false, Message = MSG_MESSAGE_SENDING_FAILED };

            if (String.IsNullOrWhiteSpace(receiverName)
                || String.IsNullOrWhiteSpace(receiverEmail)
                || String.IsNullOrWhiteSpace(subject)
                || String.IsNullOrWhiteSpace(htmlMessage)
                )
            {
                response.Message = MSG_INVALID_PARAMERTERS;
                return response;
            }

            if (!IsValidFormat(receiverEmail))
            {
                response.Message = MSG_INVALID_EMAIL_FORMAT;
                return response;
            }

            var email = new MimeMessage();
            email.From.Add(new MailboxAddress(SENDER_NAME, SENDER_EMAIL_ADDRESS));
            email.To.Add(new MailboxAddress(receiverName, receiverEmail));
            email.Subject = $"{SENDER_NAME} - {subject}";
            email.Body = new TextPart(MimeKit.Text.TextFormat.Html)
            {
                Text = htmlMessage
            };

            using (var smtp = new SmtpClient())
            {
                var smtpUsername = SENDER_EMAIL_ADDRESS;
                //var smtpUsername = (!string.IsNullOrWhiteSpace(Environment.GetEnvironmentVariable("SMTP_USERNAME")))
                //? Environment.GetEnvironmentVariable("SMTP_USERNAME") : _configuration["SMTP_USERNAME"];

                var smtpPassword = MAIL_PASSWORD;
                //var smtpPassword = (!string.IsNullOrWhiteSpace(Environment.GetEnvironmentVariable("SMTP_PASSWORD")))
                //? Environment.GetEnvironmentVariable("SMTP_PASSWORD") : _configuration["SMTP_PASSWORD"];

                smtp.Connect(SMTP_HOST, SMTP_PORT, SecureSocketOptions.StartTls);

                smtp.Authenticate(smtpUsername, smtpPassword);
                smtp.Send(email);
                smtp.Disconnect(true);

                response.Status = true;
                response.Message = MSG_MESSAGE_SENDING_SUCCESSFUL;
            }

            return response;

        }

        /// <summary>
        /// Validates an email by checking its pattern
        /// </summary>
        /// <param name="email">the email to validate</param>
        /// <returns>A boolean</returns>
        private static bool IsValidFormat(string email)
        {
            var validFormatStatus = false;
            email = email.Trim();

            if (!String.IsNullOrWhiteSpace(email))
            {
                try
                {
                    validFormatStatus = Regex.IsMatch(email, "^(?(\")(\".+?(?<!\\\\)\"@)|(([0-9a-z]((\\.(?!\\.))|[-!#\\$%&'\\*\\+/=\\?\\^`\\{\\}\\|~\\w])*)(?<=[0-9a-z])@))(?(\\[)(\\[(\\d{1,3}\\.){3}\\d{1,3}\\])|(([0-9a-z][-\\w]*[0-9a-z]*\\.)+[a-z0-9][\\-a-z0-9]{0,22}[a-z0-9]))$", RegexOptions.IgnoreCase, TimeSpan.FromMilliseconds(250.0));
                }
                catch (RegexMatchTimeoutException ex)
                {
                    // log error here later
                    //Logger.log(ex);
                    return validFormatStatus;
                }
            }
            return validFormatStatus;
        }

    }
}
