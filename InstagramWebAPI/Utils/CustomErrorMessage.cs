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
        public const string CommentRequired = "CommentText is required.";
        public const string PasswordNotmatch = "Password Not Match.";
        public const string OldPasswordRequired = "OldPassword is required.";
        public const string ConfirmPasswordRequired = "ConfirmPassword is required.";
        public const string PasswordMatch = "Password And ConfirmPassword Not Match!!.";
        public const string InvalidPasswordFormat = "Password must be 7-15 characters long and include at least one uppercase letter, one lowercase letter, one digit, and one special character.";
        public const string UsernameRequired = "Username is required.";
        public const string EmailRequired = "Email is required.";
        public const string ContactNumberRequired = "ContactNumber is required.";
        public const string InvalidUserNameFormat = "Username must start with an alphanumeric character and be between 8 to 18 characters long, allowing only letters, numbers, underscores(_), and dots(.).";
        public const string InvalidUsernameOrPassword = "Username or Password is invalid.";
        public const string DuplicateUsername = "Username already exists.";
        public const string DuplicateEmail = "Email already exists.";
        public const string DuplicateNumber = "Phone Number already exists.";
        public const string RegistrationError = "Error occurred while registering the user.";
        public const string CountError = "Error occurred while count the Follower and Follwing.";
        public const string MutualError = "Error occurred while get mutual friend.";
        public const string ModelIsNull = "All fields are required.";
        public const string LoginError = "Error occurred while logging in.";
        public const string ValidationRegistrtion = "Validation failed for registration data.";
        public const string ValidationLogin = "Validation failed for registration data."; 
        public const string RegistrationSucces = "Registration Successfully";
        public const string LoginSucces = "Login Successfully";
        public const string ExitsUser = "User Not Exits";
        public const string ExitsPOstComment = "comment Not Exits";
        public const string ExitsPost = "Post Not Exits";
        public const string ExitsStory = "Story Not Exits";
        public const string ExitsRequest = "Request Not Exits";
        public const string MailSend = "Email send successfully";
        public const string MailNotSend = "Email Not Send";
        public const string ReserError = "Error occurred while Reset Password the user.";
        public const string PostError = "Error occurred while Create Post.";
        public const string ReserPassword = "Reset Password Successfully";
        public const string ForgotPassword = "Forgot Password Successfully";
        public const string CreatePost = "Post Create Successfully";
        public const string UpdatePost = "Post Update Successfully";
        public const string ValidationReset = "Validation failed for ResetPassword data.";
        public const string InvalidUserId = "Invalid UserId. UserId must be greater than zero.";
        public const string InvalidCommentId = "Invalid CommentId. CommentId must be greater than zero.";
        public const string NullUserId = "Userid is required";
        public const string NullCommentId = "Userid is required";
        public const string MatchUserId = "Please Enter Different UserId";
        public const string NullProfilePhoto = "Profile photo is required.";
        public const string NullStoryPhoto = "Story photo is required.";
        public const string NullPostPhoto = "Post photo is required.";
        public const string InvalidPhotoExtension = "Invalid file extension. Allowed extensions are: {0}";
        public const string ValidationProfile = "Validation failed for Profile data.";
        public const string ValidationPost = "Validation failed for Post data.";
        public const string ValidationStory = "Validation failed for Story data.";
        public const string UploadPhoto = "Profile Upload Successfully";
        public const string UploadStory = "Story Upload Successfully";
        public const string SeenStory = "Story Seen Add Successfully";
        public const string LIkeStory = "Story Like Successfully";
        public const string GetStory = "Story Get Successfully";
        public const string UploadError = "Error occurred while Upload ProfilePhoto the user.";
        public const string UploadStoryError = "Error occurred while Upload Story the user.";
        public const string LikeStoryError = "Error occurred while Like Story.";
        public const string UpdateProfile = "Error occurred while Update Profile the user.";
        public const string ValidationUpdateProfile = "Validation failed for Update Profile data.";
        public const string UpdateProfileSuccess = "Profile Update Successfully";
        public const string ValidationRequest = "Validation failed for Follow Request data.";
        public const string RequestSuccess = "Request Send Successfully";
        public const string RequestError = "Error occurred while Fllow Request the user.";
        public const string NullDateOfBirth = "Date of birth is required";
        public const string InvalidDateOfBirthFormat = "Date of birth should be in yyyy-MM-dd format";
        public const string FutureDateOfBirth = "Please enter valid DateOfBirth";
        public const string InvalidType = "Please enter valid Type of List";
        public const string InvalidReqType = "Please enter valid Type of Request";
        public const string InvalidPostType = "Please enter valid Type of Post";
        public const string ValidationList = "Validation failed for Get List data.";
        public const string GetFollowerList = "Error occurred while Get List .";
        public const string GetFollowerListSucces = "List Get Successfully";
        public const string GetPostListSucces = "Post List Get Successfully";
        public const string GetUser = "User Get Successfully";
        public const string NullRequestId = "Requestid is required";
        public const string NullPostId = "PostId is required";
        public const string NullStoryId = "StoryId is required";
        public const string InvalidRequestId = "Invalid RequestId. requestId must be greater than zero.";
        public const string InvalidPostId = "Invalid PostId.PostId must be greater than zero.";
        public const string InvalidStoryId = "Invalid StoryId.StoryId must be greater than zero.";
        public const string ValidationReqType = "Validation failed for Request  data.";
        public const string AccpteUpdate = "Update Successfully";
        public const string PostDelete = "Post Delete Successfully";
        public const string StoryDelete = "Story Delete Successfully";
        public const string PostCommentDelete = "Comment Delete Successfully";
        public const string PostLike = "Post Like Or Unlike Successfully";
        public const string PostComment = "Post Comment Successfully";
        public const string CountSucces = "Get Count Successfully";
        public const string mutualSucces = "Mutual Friend List Get Successfully";
        public const string PostDeleteError = "Error occurred while Delete Post.";
        public const string StoryDeleteError = "Error occurred while Delete Story.";
        public const string PostLikeError = "Error occurred while Like Post.";
        public const string PostCommentError = "Error occurred while Comment Post.";
        public const string PathNotExits = "Path Not Found";

















    }
}
