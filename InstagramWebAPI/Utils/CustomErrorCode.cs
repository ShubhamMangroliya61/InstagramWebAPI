namespace InstagramWebAPI.Utils
{
    public class CustomErrorCode
    {
        public const string IsValid = "VALIDATION_ERROR";
        public const string IsUserName = "DUPLICATE_USERNAME";
        public const string IsRegister = "REGISTRATION_ERROR";
        public const string IsLogin = "INVALID_USERNAME_OR_PASSWORD";
        public const string LoginError = "LOGIN_ERROR";
        public const string ModelIsNull = "MODEL_IS_NULL";
        public const string NullEmailOrMobileNumber = "NULL_EMAIL_OR_MOBILENUMBER";
        public const string InvalidEmailFormat = "INVALID_EMAIL_FORMAT";
        public const string InvalidMobileNumberFormat = "INVALID_MOBILENUMBER_FORMAT";
        public const string NullPassword = "NULL_PASSWORD";
        public const string PasswordNOTMatch = "PASSWORD_NOT_MATCH";
        public const string NullConfirmPassword = "NULL_CONFIRMPASSWORD";
        public const string InvalidPasswordFormat = "INVALID_PASSWORD_FORMAT";
        public const string NullUserName = "NULL_USERNAME";
        public const string NullEmailOrMobileNumberOrUsername = "NULL_EMAIL_OR_MOBILENUMBER_OR_USERNAME";
        public const string IsNotExits = "USER_NOT_EXITS";
        public const string MailNotSend = "MAIL_NOT_SEND";
        public const string IsReset = "RESET_ERROR";

    }
}
