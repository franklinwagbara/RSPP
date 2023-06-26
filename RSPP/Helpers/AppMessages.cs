namespace RSPP.Helpers
{
    public static class AppMessages
    {
        public const string EMAIL = "Email";
        public const string COMPANY = "COMPANY";
        public const string ADMIN = "ADMIN";
        public const string USER = "USER";
        public const string LOGIN = "Login";
        public const string REGISTRATION = "Registration";
        public const string AUTHENTICATION = "Authentication";

        public const string NOT_EXIST = "does not exist";
        public const string EXISTS = "already exists";
        public const string CONFIRMATION = "confirmation";

        #region Authentication
        public const string ACTIVE = "ACTIVE";
        public const string PASSIVE = "PASSIVE";
        public const string SUCCESSFUL = "successful";
        public const string FAILED = "failed";
        public const string INVALID_USERNAME_PASSWORD = "Invalid username or password";
        #endregion

        #region Email
        public const string SENDING = "sending"; 
        public const string EMAIL_VERIFICATION_LINK_SENT = "A link has been sent to the provided email. Click the link to verify your email address"; 
        public const string EMAIL_VERIFICATION_LINK_FAILED = "Unable to send email verification link to provided email"; 


        #endregion

    }
}
