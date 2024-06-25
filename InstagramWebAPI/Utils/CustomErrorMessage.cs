namespace InstagramWebAPI.Utils
{
    public class CustomErrorMessage
    {
        // Validation errors
        public const string EmailOrMobileOrUsernameRequired = "Either Email, MobileNumber, or UserName is required.";
        public const string EmailOrMobileRequired = "Either Email, MobileNumber is required.";
        public const string InvalidEmailFormat = "Please enter a valid Email address.";
        public const string InvalidMobileNumberFormat = "Please enter a valid Mobile number.";
        public const string PasswordRequired = "Password is required.";
        public const string ConfirmPasswordRequired = "ConfirmPassword is required.";
        public const string PasswordMatch = "Password And ConfirmPassword Not Match!!.";
        public const string InvalidPasswordFormat = "Password must be 7-15 characters long and include at least one uppercase letter, one lowercase letter, one digit, and one special character.";
        public const string UsernameRequired = "Username is required.";
        public const string InvalidUserNameFormat = "Username must start with an alphanumeric character and be between 8 to 18 characters long, allowing only letters, numbers, underscores(_), and dots(.).";
        public const string InvalidUsernameOrPassword = "Username or Password is invalid.";
        public const string DuplicateUsername = "Username already exists.";
        public const string RegistrationError = "Error occurred while registering the user.";
        public const string ModelIsNull = "All fields are required.";
        public const string LoginError = "Error occurred while logging in.";
        public const string ValidationRegistrtion = "Validation failed for registration data.";
        public const string ValidationLogin = "Validation failed for registration data."; 
        public const string RegistrationSucces = "Registration Successfully";
        public const string LoginSucces = "Login Successfully";
        public const string ExitsUser = "User Not Exits";
        public const string MailSend = "Email send successfully";
        public const string MailNotSend = "Email Not Send";
        public const string ReserError = "Error occurred while Reset Password the user.";
        public const string ReserPassword = "Reset Password Successfully";
        public const string ValidationReset = "Validation failed for ResetPassword data.";
        public const string InvalidUserId = "Invalid UserId. UserId must be greater than zero.";
        public const string NullProfilePhoto = "Profile photo is required.";
        public const string InvalidPhotoExtension = "Invalid file extension. Allowed extensions are: {0}";
        public const string ValidationProfile = "Validation failed for Profile data.";
        public const string UploadPhoto = "Profile Upload Successfully";
        public const string UploadError = "Error occurred while Upload ProfilePhoto the user.";
        public const string UpdateProfile = "Error occurred while Update Profile the user.";
        public const string ValidationUpdateProfile = "Validation failed for Update Profile data.";
        public const string UpdateProfileSuccess = "Profile Update Successfully";
        public const string ValidationRequest = "Validation failed for Follow Request data.";
        public const string RequestSuccess = "Request Send Successfully";
        public const string RequestError = "Error occurred while Fllow Request the user.";









    }
}
