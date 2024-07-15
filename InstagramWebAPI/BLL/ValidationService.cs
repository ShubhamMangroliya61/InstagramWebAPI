using DataAccess.CustomModel;
using InstagramWebAPI.Common;
using InstagramWebAPI.DAL.Models;
using InstagramWebAPI.DTO;
using InstagramWebAPI.Interface;
using InstagramWebAPI.Utils;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Globalization;
using System.Reflection.Metadata;
using System.Text.RegularExpressions;
using static Org.BouncyCastle.Crypto.Digests.SkeinEngine;
using static System.Net.WebRequestMethods;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace InstagramWebAPI.BLL
{
    public class ValidationService : IValidationService
    {
        private static readonly string[] AllowedExtensionsProfilePhoto = { ".jpg", ".jpeg", ".png" };
        private static readonly string[] AllowedExtensions = { ".jpg", ".jpeg", ".png", ".mp4" };
        public readonly ApplicationDbContext _dbcontext;
        public List<ValidationError> errors;
        public ValidationService(ApplicationDbContext dbcontext)
        {
            _dbcontext = dbcontext;
            this.errors = new();
        }

        const string EmailRegex = @"^[\w\-\.]+@([\w-]+\.)+[\w-]{2,4}$";
        const string MobileRegex = @"^[6-9]{1}[0-9]{9}$";
        const string PasswordRegex = @"^(?=.*[A-Z])(?=.*\d)(?=.*[a-z])(?=.*\W).{7,15}$";
        const string UserNameRegex = @"^[a-zA-Z0-9][a-zA-Z0-9_.]{7,17}$";
        const string LinkRegex = @"^(ftp|http|https):\/\/[^""\s]+(?:\/[^""\s]*)?$";

        public void ValidateEmail(string email)
        {
            if (string.IsNullOrWhiteSpace(email))
            {
                errors.Add(new ValidationError
                {
                    message = CustomErrorMessage.EmailRequired,
                    reference = "email",
                    parameter = "email",
                    errorCode = CustomErrorCode.NullEmail
                });
            }
            else if (!Regex.IsMatch(email, EmailRegex))
            {
                errors.Add(new ValidationError
                {
                    message = CustomErrorMessage.InvalidEmailFormat,
                    reference = "email",
                    parameter = "email",
                    errorCode = CustomErrorCode.InvalidEmailFormat
                });
            }
        }
        public List<ValidationError> ValidateDateOfBirth(string dateOfBirth)
        {
            if (string.IsNullOrWhiteSpace(dateOfBirth))
            {
                errors.Add(new ValidationError()
                {
                    parameter = "DateOfBirth",
                    reference = null,
                    errorCode = CustomErrorCode.NullDateOfBirth,
                    message = CustomErrorMessage.NullDateOfBirth
                });
            }
            else
            {
                string[] dateFormats = {
                                            "yyyy-MM-dd",    // 2024-07-08
                                            "dd-MM-yyyy",    // 08-07-2024
                                            "MM/dd/yyyy",    // 07/08/2024
                                            "yyyy/MM/dd",    // 2024/07/08
                                            "dd MMM yyyy",   // 08 Jul 2024
                                            "dd MMMM yyyy",  // 08 July 2024
                                            "MMM dd, yyyy",  // Jul 08, 2024
                                            "MMMM dd, yyyy"  // July 08, 2024
                                        };
                if (!DateTime.TryParseExact(dateOfBirth, dateFormats, CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime date))
                {
                    errors.Add(new ValidationError()
                    {
                        parameter = "DateOfBirth",
                        reference = null,
                        errorCode = CustomErrorCode.InvalidDateOfBirthFormat,
                        message = CustomErrorMessage.InvalidDateOfBirthFormat
                    });
                }
                else
                {
                    // Check if the date is after today (future date)
                    if (date > DateTime.Today)
                    {
                        errors.Add(new ValidationError()
                        {
                            parameter = "DateOfBirth",
                            reference = null,
                            errorCode = CustomErrorCode.FutureDateOfBirth,
                            message = CustomErrorMessage.FutureDateOfBirth
                        });
                    }
                }
            }
            return errors;
        }
        public void ValidateUserName(string userName)
        {
            if (string.IsNullOrWhiteSpace(userName))
            {
                errors.Add(new ValidationError
                {
                    message = CustomErrorMessage.UsernameRequired,
                    reference = "UserName",
                    parameter = "UserName",
                    errorCode = CustomErrorCode.NullUserName
                });
            }
            else if (!Regex.IsMatch(userName, UserNameRegex))
            {
                errors.Add(new ValidationError
                {
                    message = CustomErrorMessage.InvalidUserNameFormat,
                    reference = "UserName",
                    parameter = "UserName",
                    errorCode = CustomErrorCode.InvalidUserNameFormat
                });
            }
        }
        public void ValidatePassword(string password)
        {
            if (string.IsNullOrWhiteSpace(password))
            {
                errors.Add(new ValidationError
                {
                    message = CustomErrorMessage.PasswordRequired,
                    reference = "Password",
                    parameter = "Password",
                    errorCode = CustomErrorCode.NullPassword
                });
            }
            else if (!Regex.IsMatch(password, PasswordRegex))
            {
                errors.Add(new ValidationError
                {
                    message = CustomErrorMessage.InvalidPasswordFormat,
                    reference = "Password",
                    parameter = "Password",
                    errorCode = CustomErrorCode.InvalidPasswordFormat
                });
            }
        }
        public void ValidateContactNumber(string contactNumber)
        {
            if (!string.IsNullOrWhiteSpace(contactNumber) && !Regex.IsMatch(contactNumber, MobileRegex))
            {
                errors.Add(new ValidationError
                {
                    message = CustomErrorMessage.InvalidMobileNumberFormat,
                    reference = "contactNumber",
                    parameter = "contactNumber",
                    errorCode = CustomErrorCode.InvalidMobileNumberFormat
                });
            }
        }
        public List<ValidationError> ValidateUserId(long userId)
        {
            if (userId == 0)
            {
                errors.Add(new ValidationError
                {
                    message = CustomErrorMessage.NullUserId,
                    reference = "UserId",
                    parameter = "UserId",
                    errorCode = CustomErrorCode.NullUserId
                });
            }
            else if (userId < 0)
            {
                errors.Add(new ValidationError
                {
                    message = CustomErrorMessage.InvalidUserId,
                    reference = "UserId",
                    parameter = "UserId",
                    errorCode = CustomErrorCode.InvalidUserId
                });
            }
            if (!_dbcontext.Users.Any(m => m.UserId == userId && m.IsDeleted != true))
            {
                errors.Add(new ValidationError
                {
                    message = CustomErrorMessage.ExitsUser,
                    reference = "userid",
                    parameter = "userid",
                    errorCode = CustomErrorCode.IsNotExits
                });
            }
            return errors;
        }
        public List<ValidationError> ValidateRequestId(long requestId)
        {
            if (requestId == 0)
            {
                errors.Add(new ValidationError
                {
                    message = CustomErrorMessage.NullRequestId,
                    reference = "requestId",
                    parameter = "requestId",
                    errorCode = CustomErrorCode.NullRequestId
                });
            }
            else if (requestId < 0)
            {
                errors.Add(new ValidationError
                {
                    message = CustomErrorMessage.InvalidRequestId,
                    reference = "requestId",
                    parameter = "requestId",
                    errorCode = CustomErrorCode.InvalidRequestId
                });
            }
            else if (!_dbcontext.Requests.Any(m => m.RequestId == requestId && m.IsDeleted != true))
            {
                errors.Add(new ValidationError
                {
                    message = CustomErrorMessage.ExitsRequest,
                    reference = "requestId",
                    parameter = "requestId",
                    errorCode = CustomErrorCode.IsNotRequest
                });
            }
            return errors;
        }
        public List<ValidationError> ValidatePostId(long postId)
        {
            if (postId == 0)
            {
                errors.Add(new ValidationError
                {
                    message = CustomErrorMessage.NullPostId,
                    reference = "postid",
                    parameter = "postid",
                    errorCode = CustomErrorCode.NullPostId
                });
            }
            else if (postId < 0)
            {
                errors.Add(new ValidationError
                {
                    message = CustomErrorMessage.InvalidPostId,
                    reference = "postid",
                    parameter = "postid",
                    errorCode = CustomErrorCode.InvalidPostId
                });
            }
            else if (!_dbcontext.Posts.Any(m => m.PostId == postId && m.IsDeleted != true))
            {
                errors.Add(new ValidationError
                {
                    message = CustomErrorMessage.ExitsPost,
                    reference = "postid",
                    parameter = "postid",
                    errorCode = CustomErrorCode.IsNotPost
                });
            }
            return errors;
        }
        public List<ValidationError> ValidateHighLightId(long highLightId)
        {
            if (highLightId == 0)
            {
                errors.Add(new ValidationError
                {
                    message = CustomErrorMessage.NullHighLighttId,
                    reference = "highLightId",
                    parameter = "highLightId",
                    errorCode = CustomErrorCode.NullHighLightId
                });
            }
            else if (highLightId < 0)
            {
                errors.Add(new ValidationError
                {
                    message = CustomErrorMessage.InvalidHighLightId,
                    reference = "highLightId",
                    parameter = "highLightId",
                    errorCode = CustomErrorCode.InvalidHighLightId
                });
            }
            else if (!_dbcontext.Highlights.Any(m => m.HighlightsId == highLightId && m.IsDeleted != true))
            {
                errors.Add(new ValidationError
                {
                    message = CustomErrorMessage.ExitHighlight,
                    reference = "highLightId",
                    parameter = "highLightId",
                    errorCode = CustomErrorCode.IsNotHighlight
                });
            }
            return errors;
        }
        public List<ValidationError> ValidateColletionId(long collectionId)
        {
            if (collectionId == 0)
            {
                errors.Add(new ValidationError
                {
                    message = CustomErrorMessage.NullCollectionId,
                    reference = "highLightId",
                    parameter = "highLightId",
                    errorCode = CustomErrorCode.NullCollectionId
                });
            }
            else if (!_dbcontext.Collections.Any(m => m.CollectionId == collectionId && m.IsDeleted != true))
            {
                errors.Add(new ValidationError
                {
                    message = CustomErrorMessage.ExitCollection,
                    reference = "highLightId",
                    parameter = "highLightId",
                    errorCode = CustomErrorCode.IsNotCollection
                });
            }
            return errors;
        }
        public List<ValidationError> ValidateSerachId(long searchId)
        {
            if (searchId == 0)
            {
                errors.Add(new ValidationError
                {
                    message = CustomErrorMessage.NullsearchId,
                    reference = "highLightId",
                    parameter = "highLightId",
                    errorCode = CustomErrorCode.NullSearchId
                });
            }
            else if (!_dbcontext.Searches.Any(m => m.SearchId == searchId && m.IsDeleted != true))
            {
                errors.Add(new ValidationError
                {
                    message = CustomErrorMessage.ExitSearch,
                    reference = "highLightId",
                    parameter = "highLightId",
                    errorCode = CustomErrorCode.IsNotSearchId
                });
            }
            return errors;
        }
        public List<ValidationError> ValidateStoryHighLightId(long storyHighLightId)
        {
            if (storyHighLightId == 0)
            {
                errors.Add(new ValidationError
                {
                    message = CustomErrorMessage.NullStoryHighLighttId,
                    reference = "highLightId",
                    parameter = "highLightId",
                    errorCode = CustomErrorCode.NullStoryHighLightId
                });
            }
            else if (storyHighLightId < 0)
            {
                errors.Add(new ValidationError
                {
                    message = CustomErrorMessage.InvalidStoryHighLightId,
                    reference = "highLightId",
                    parameter = "highLightId",
                    errorCode = CustomErrorCode.InvalidStoryHighLightId
                });
            }
            else if (!_dbcontext.StoryHighlights.Any(m => m.StoryHighlightId == storyHighLightId && m.IsDeleted != true))
            {
                errors.Add(new ValidationError
                {
                    message = CustomErrorMessage.ExitStoryHighlight,
                    reference = "highLightId",
                    parameter = "highLightId",
                    errorCode = CustomErrorCode.IsNotStoryHighlight
                });
            }
            return errors;
        }

        public List<ValidationError> validatePostCollectionId(long postCollectionId)
        {
            if (postCollectionId == 0)
            {
                errors.Add(new ValidationError
                {
                    message = CustomErrorMessage.NullPostCollectionId,
                    reference = "highLightId",
                    parameter = "highLightId",
                    errorCode = CustomErrorCode.NullPostCollectionId
                });
            }
            else if (!_dbcontext.PostCollections.Any(m => m.PostCollectionId == postCollectionId && m.IsDeleted != true))
            {
                errors.Add(new ValidationError
                {
                    message = CustomErrorMessage.ExitPostCollection,
                    reference = "highLightId",
                    parameter = "highLightId",
                    errorCode = CustomErrorCode.IsNotPostCollection
                });
            }
            return errors;
        }
        /// <summary>
        /// Checks if the given username is unique.
        /// </summary>
        /// <param name="userName">The username to check.</param>
        /// <returns>True if the username is unique; false otherwise.</returns>
        public bool IsUniqueUserName(string userName, long userId)
        {
            User? user = _dbcontext.Users.FirstOrDefault(m => ((m.UserName ?? string.Empty).ToLower() == (userName ?? string.Empty).ToLower() && !string.IsNullOrWhiteSpace(m.UserName)) && m.IsDeleted == false && (m.UserId <= 0 || m.UserId != userId));
            if (user == null) return false;

            return true;
        }

        /// <summary>
        /// Checks if the given username is unique.
        /// </summary>
        /// <param name="userName">The username to check.</param>
        /// <returns>True if the username is unique; false otherwise.</returns>
        public bool IsUniqueEmail(UserDTO model)
        {
            User? user = _dbcontext.Users.FirstOrDefault(m => ((m.Email ?? string.Empty).ToLower() == (model.Email ?? string.Empty).ToLower() && !string.IsNullOrWhiteSpace(m.Email))
                                       && m.IsDeleted != true && (m.UserId <= 0 || m.UserId != model.UserId));
            if (user == null) return false;

            return true;
        }

        /// <summary>
        /// Checks if the given username is unique.
        /// </summary>
        /// <param name="userName">The username to check.</param>
        /// <returns>True if the username is unique; false otherwise.</returns>
        public bool IsUniquePhoneNumber(UserDTO model)
        {
            User? user = _dbcontext.Users.FirstOrDefault(m => (m.ContactNumber == model.ContactNumber && !string.IsNullOrWhiteSpace(m.ContactNumber) && !string.IsNullOrWhiteSpace(model.ContactNumber))
                                       && m.IsDeleted != true && (m.UserId <= 0 || m.UserId != model.UserId));
            if (user == null) return false;

            return true;
        }

        /// <summary>
        /// Validates the registration request DTO.
        /// </summary>
        /// <param name="model">The <see cref="RegistrationRequestDTO"/> containing registration information.</param>
        /// <returns>A list of <see cref="ValidationError"/> objects indicating validation errors.</returns>
        public List<ValidationError> ValidateRegistration(UserDTO model)
        {
            if (model.UserId == 0)
            {
                ValidatePassword(model.Password ?? string.Empty);
            }
            ValidateEmail(model.Email ?? string.Empty);
            ValidateContactNumber(model.ContactNumber ?? string.Empty);
            ValidateUserName(model.UserName);
            if (IsUniqueUserName(model.UserName, model.UserId))
            {
                errors.Add(new ValidationError
                {
                    message = CustomErrorMessage.DuplicateUsername,
                    reference = "username",
                    parameter = model.UserName,
                    errorCode = CustomErrorCode.IsUserName
                });
            }
            if (IsUniqueEmail(model))
            {
                errors.Add(new ValidationError
                {
                    message = CustomErrorMessage.DuplicateEmail,
                    reference = "Email",
                    parameter = model.Email,
                    errorCode = CustomErrorCode.IsEmail
                });

            }
            if (IsUniquePhoneNumber(model))
            {
                errors.Add(new ValidationError
                {
                    message = CustomErrorMessage.DuplicateNumber,
                    reference = "mobileNumber",
                    parameter = model.ContactNumber,
                    errorCode = CustomErrorCode.IsPhoneNumber
                });
            }
            if (model.UserId > 0)
            {
                ValidateDateOfBirth(model.DateOfBirth ?? string.Empty);
                ValidateUserId(model.UserId);
                if (!string.IsNullOrWhiteSpace(model.Link) && !Regex.IsMatch(model.Link, LinkRegex, RegexOptions.IgnoreCase))
                {
                    errors.Add(new ValidationError
                    {
                        message = CustomErrorMessage.InvalidLink,
                        reference = "UserName",
                        parameter = "UserName",
                        errorCode = CustomErrorCode.InvalidLink
                    });
                }
            }
            return errors;
        }

        /// <summary>
        /// Validates the login request DTO.
        /// </summary>
        /// <param name="model">The <see cref="LoginRequestDTO"/> containing login information.</param>
        /// <returns>A list of <see cref="ValidationError"/> objects indicating validation errors.</returns>
        public List<ValidationError> ValidateLogin(LoginRequestDTO model)
        {

            if (string.IsNullOrWhiteSpace(model.UserID))
            {
                errors.Add(new ValidationError
                {
                    message = CustomErrorMessage.EmailOrMobileOrUsernameRequired,
                    reference = "Email OR MobileNumber OR UserName",
                    parameter = "Email or MobileNumber or UserName",
                    errorCode = CustomErrorCode.NullEmailOrMobileNumberOrUsername
                });
            }
            else
            {
                if (model.Type == "email" && !Regex.IsMatch(model.UserID, EmailRegex))
                {
                    errors.Add(new ValidationError
                    {
                        message = CustomErrorMessage.InvalidEmailFormat,
                        reference = "Email",
                        parameter = model.UserID,
                        errorCode = CustomErrorCode.InvalidEmailFormat
                    });
                }
                else if (model.Type == "phone" && !Regex.IsMatch(model.UserID, MobileRegex))
                {
                    errors.Add(new ValidationError
                    {
                        message = CustomErrorMessage.InvalidMobileNumberFormat,
                        reference = "MobileNumber",
                        parameter = model.UserID,
                        errorCode = CustomErrorCode.InvalidMobileNumberFormat
                    });
                }
                else if (model.Type == "username" && !Regex.IsMatch(model.UserID, UserNameRegex))
                {
                    errors.Add(new ValidationError
                    {
                        message = CustomErrorMessage.InvalidUserNameFormat,
                        reference = "username",
                        parameter = "username",
                        errorCode = CustomErrorCode.InvalidUserNameFormat
                    });
                }

                ValidatePassword(model.Password ?? string.Empty);
            }

            return errors;
        }

        /// <summary>
        /// Validates the reset password request DTO.
        /// </summary>
        /// <param name="model">The reset password request DTO containing email or mobile number.</param>
        /// <returns>A list of validation errors, if any.</returns>
        public List<ValidationError> ValidateForgotPassword(ForgotPasswordDTO model)
        {

            if (string.IsNullOrWhiteSpace(model.EmailOrNumberOrUserName))
            {
                errors.Add(new ValidationError
                {
                    message = CustomErrorMessage.EmailOrMobileOrUsernameRequired,
                    reference = "Email OR MobileNumber OR UserName",
                    parameter = "Email or MobileNumber or UserName",
                    errorCode = CustomErrorCode.NullEmailOrMobileNumberOrUsername
                });
            }
            else
            {
                if (model.Type == "email" && !Regex.IsMatch(model.EmailOrNumberOrUserName, EmailRegex))
                {
                    errors.Add(new ValidationError
                    {
                        message = CustomErrorMessage.InvalidEmailFormat,
                        reference = "Email",
                        parameter = "email",
                        errorCode = CustomErrorCode.InvalidEmailFormat
                    });
                }
                else if (model.Type == "phone" && !Regex.IsMatch(model.EmailOrNumberOrUserName, MobileRegex))
                {
                    errors.Add(new ValidationError
                    {
                        message = CustomErrorMessage.InvalidMobileNumberFormat,
                        reference = "MobileNumber",
                        parameter = "MobileNumber",
                        errorCode = CustomErrorCode.InvalidMobileNumberFormat
                    });
                }
                else if (model.Type == "username" && !Regex.IsMatch(model.EmailOrNumberOrUserName, UserNameRegex))
                {
                    errors.Add(new ValidationError
                    {
                        message = CustomErrorMessage.InvalidUserNameFormat,
                        reference = "UserName",
                        parameter = "UserName",
                        errorCode = CustomErrorCode.InvalidUserNameFormat
                    });
                }
            }
            return errors;
        }

        /// <summary>
        /// Validates the reset password data including password and confirm password fields.
        /// </summary>
        /// <param name="model">The reset password DTO containing password and confirm password fields.</param>
        /// <returns>A list of validation errors, if any.</returns>
        public List<ValidationError> ValidateForgotPasswordData(ForgotPasswordDTO model)
        {
            ValidateUserId(model.UserId);
            ValidatePassword(model.Password ?? string.Empty);

            if (string.IsNullOrWhiteSpace(model.ConfirmPassword))
            {
                errors.Add(new ValidationError
                {
                    message = CustomErrorMessage.ConfirmPasswordRequired,
                    reference = "Password",
                    parameter = "Password",
                    errorCode = CustomErrorCode.NullConfirmPassword
                });
            }
            else if (model.ConfirmPassword != model.Password)
            {
                errors.Add(new ValidationError
                {
                    message = CustomErrorMessage.PasswordMatch,
                    reference = "Password",
                    parameter = "Password",
                    errorCode = CustomErrorCode.PasswordNOTMatch
                });
            }
            return errors;
        }

        /// <summary>
        /// Validates the profile photo upload data, including user ID presence and allowed file extensions.
        /// </summary>
        /// <param name="model">The upload profile DTO containing user ID and profile photo information.</param>
        /// <returns>A list of validation errors, if any.</returns>
        public List<ValidationError> ValidateProfileFile(IFormFile ProfilePhoto, long userId)
        {
            List<ValidationError> errors = new();

            ValidateUserId(userId);

            if (ProfilePhoto != null)
            {
                string fileExtension = Path.GetExtension(ProfilePhoto.FileName).ToLowerInvariant();
                if (!AllowedExtensionsProfilePhoto.Contains(fileExtension))
                {
                    errors.Add(new ValidationError
                    {
                        message = string.Format(CustomErrorMessage.InvalidPhotoExtension, string.Join(", ", AllowedExtensionsProfilePhoto)),
                        reference = "ProfilePhoto",
                        parameter = "ProfilePhoto",
                        errorCode = CustomErrorCode.InvalidPhotoExtension
                    });
                }

                int maxFileSizeInBytes = 1024 * 1024;
                if (ProfilePhoto.Length > maxFileSizeInBytes)
                {
                    errors.Add(new ValidationError
                    {
                        message = CustomErrorMessage.FileSizeLimitExceeded,
                        reference = "ProfilePhoto",
                        parameter = "ProfilePhoto",
                        errorCode = CustomErrorCode.FileSizeLimitExceeded
                    });
                }
            }

            return errors;
        }
        public List<ValidationError> ValidateProfileData(UserDTO model)
        {
            List<ValidationError> errors = new();
            const string UserNameRegex = @"^[a-zA-Z0-9][a-zA-Z0-9_.]{7,17}$";


            if (model.UserId <= 0)
            {
                errors.Add(new ValidationError
                {
                    message = CustomErrorMessage.InvalidUserId,
                    reference = "UserId",
                    parameter = "UserId",
                    errorCode = CustomErrorCode.InvalidUserId
                });
            }
            if (string.IsNullOrWhiteSpace(model.UserName))
            {
                errors.Add(new ValidationError
                {
                    message = CustomErrorMessage.UsernameRequired,
                    reference = "UserName",
                    parameter = "UserName",
                    errorCode = CustomErrorCode.NullUserName
                });
            }
            else if (!Regex.IsMatch(model.UserName, UserNameRegex))
            {
                errors.Add(new ValidationError
                {
                    message = CustomErrorMessage.InvalidUserNameFormat,
                    reference = "username",
                    parameter = "username",
                    errorCode = CustomErrorCode.InvalidUserNameFormat
                });
            }
            return errors;
        }
        public List<ValidationError> ValidateFollowRequest(FollowRequestDTO model ,long fromUserId)
        {
            if (fromUserId== model.ToUserId)
            {
                errors.Add(new ValidationError
                {
                    message = CustomErrorMessage.MatchUserId,
                    reference = "UserId",
                    parameter = "UserId",
                    errorCode = CustomErrorCode.SameUserId
                });
            }
            
            if (model.ToUserId <= 0)
            {
                errors.Add(new ValidationError
                {
                    message = CustomErrorMessage.InvalidUserId,
                    reference = "UserId",
                    parameter = "UserId",
                    errorCode = CustomErrorCode.InvalidToUserId
                });
            }
            return errors;
        }
        public List<ValidationError> ValidateFollowerList(RequestDTO<FollowerListRequestDTO> model)
        {
            ValidateUserId(model.Model.UserId);
            if (!(model.Model.FollowerOrFollowing == "Follower" || model.Model.FollowerOrFollowing == "Following"))
            {
                errors.Add(new ValidationError
                {
                    message = CustomErrorMessage.InvalidType,
                    reference = "type",
                    parameter = "type",
                    errorCode = CustomErrorCode.InvalidListType
                });
            }
            return errors;
        }
        public List<ValidationError> ValidateRequestList(RequestDTO<FollowRequestDTO> model)
        {
            ValidateUserId(model.Model.UserId);
            return errors;
        }
        public List<ValidationError> ValidateGetUserById(long userId)
        {
            ValidateUserId(userId);
            return errors;
        }
        public List<ValidationError> ValidateMatualFrnd(RequestDTO<UserIdRequestDTO> model)
        {
            ValidateUserId(model.Model.UserId);
            return errors;
        }
        public List<ValidationError> ValidateRequestAccept(long requestId, string acceptType)
        {
            ValidateRequestId(requestId);
            if (!(acceptType == "Accept" || acceptType == "Cancel"))
            {
                errors.Add(new ValidationError
                {
                    message = CustomErrorMessage.InvalidReqType,
                    reference = "reqtype",
                    parameter = "reqtype",
                    errorCode = CustomErrorCode.InvalidReqType
                });
            }
            return errors;
        }
        public List<ValidationError> ValidateCreatePost(CreatePostDTO model)
        {
            if (model.PostId > 0)
            {
                if (!_dbcontext.Posts.Any(m => m.PostId == model.PostId && m.IsDeleted != true))
                {
                    errors.Add(new ValidationError
                    {
                        message = CustomErrorMessage.ExitsPost,
                        reference = "postid",
                        parameter = "postid",
                        errorCode = CustomErrorCode.IsNotPost
                    });
                }
            }
            if (model.PostId <= 0)
            {
                if (model.File == null || model.File.Count == 0)
                {
                    errors.Add(new ValidationError
                    {
                        message = CustomErrorMessage.NullPostPhoto,
                        reference = "Files",
                        parameter = "Files",
                        errorCode = CustomErrorCode.NullProfilePhoto
                    });
                }
                else
                {
                    foreach (var file in model.File)
                    {
                        string fileExtension = Path.GetExtension(file.FileName).ToLowerInvariant();
                        long fileSizeInBytes = file.Length;

                        if (!AllowedExtensions.Contains(fileExtension))
                        {
                            errors.Add(new ValidationError
                            {
                                message = string.Format(CustomErrorMessage.InvalidPhotoExtension, string.Join(", ", AllowedExtensionsProfilePhoto)),
                                reference = "Files",
                                parameter = "Files",
                                errorCode = CustomErrorCode.InvalidFileFormat
                            });
                        }
                        if (file.ContentType.Contains("image"))
                        {
                            // Photo file size limit (1 MB)
                            if (fileSizeInBytes > 1 * 1024 * 1024) // 1 MB in bytes
                            {
                                errors.Add(new ValidationError
                                {
                                    message = CustomErrorMessage.FileSizeLimitExceeded,
                                    reference = "Files",
                                    parameter = "Files",
                                    errorCode = CustomErrorCode.FileSizeLimitExceeded
                                });
                            }
                        }
                        else if (file.ContentType.Contains("video"))
                        {
                            // Video file size limit (3 MB less than)
                            if (fileSizeInBytes > 3 * 1024 * 1024) // 3 MB in bytes
                            {
                                errors.Add(new ValidationError
                                {
                                    message = CustomErrorMessage.VideoFileSizeLimitExceeded,
                                    reference = "Files",
                                    parameter = "Files",
                                    errorCode = CustomErrorCode.FileSizeLimitExceeded
                                });
                            }
                        }
                    }
                }
            }
            if (!(model.PostType == "Post" || model.PostType == "Reel"))
            {
                errors.Add(new ValidationError
                {
                    message = CustomErrorMessage.InvalidPostType,
                    reference = "post",
                    parameter = "post",
                    errorCode = CustomErrorCode.InvalidPostType
                });
            }
            return errors;
        }
        public List<ValidationError> ValidateResetPassword(ResetPasswordRequestDTO model)
        {
            ValidateUserId(model.UserId);
            if (string.IsNullOrWhiteSpace(model.OldPassword))
            {
                errors.Add(new ValidationError
                {
                    message = CustomErrorMessage.OldPasswordRequired,
                    reference = "Password",
                    parameter = "Password",
                    errorCode = CustomErrorCode.NullOldPassword
                });
            }
            else if (!Regex.IsMatch(model.OldPassword, PasswordRegex))
            {
                errors.Add(new ValidationError
                {
                    message = CustomErrorMessage.InvalidPasswordFormat,
                    reference = "Password",
                    parameter = "Password",
                    errorCode = CustomErrorCode.InvalidOldPasswordFormat
                });
            }
            User? user = _dbcontext.Users.FirstOrDefault(m => m.UserId == model.UserId && m.IsDeleted == false);
            if (user != null && !BCrypt.Net.BCrypt.Verify(model.OldPassword, user.Password))
            {
                errors.Add(new ValidationError
                {
                    message = CustomErrorMessage.PasswordNotmatch,
                    reference = "Password",
                    parameter = "Password",
                    errorCode = CustomErrorCode.PasswordNotMatch
                });
            }
            ValidatePassword(model.Password ?? string.Empty);
            if (string.IsNullOrWhiteSpace(model.ConfirmPassword))
            {
                errors.Add(new ValidationError
                {
                    message = CustomErrorMessage.ConfirmPasswordRequired,
                    reference = "Password",
                    parameter = "Password",
                    errorCode = CustomErrorCode.NullConfirmPassword
                });
            }
            else if (model.ConfirmPassword != model.Password)
            {
                errors.Add(new ValidationError
                {
                    message = CustomErrorMessage.PasswordMatch,
                    reference = "Password",
                    parameter = "Password",
                    errorCode = CustomErrorCode.PasswordNOTMatch
                });
            }
            return errors;
        }
        public List<ValidationError> ValidatePostList(RequestDTO<PostListRequestDTO> model)
        {
            ValidateUserId(model.Model.UserId);
            if (!(model.Model.PostType == "Post" || model.Model.PostType == "Reel"))
            {
                errors.Add(new ValidationError
                {
                    message = CustomErrorMessage.InvalidPostType,
                    reference = "typepost",
                    parameter = "typepost",
                    errorCode = CustomErrorCode.InvalidPostType
                });
            }
            return errors;
        }
        public List<ValidationError> ValidatePostById(long postId, string postType)
        {
            ValidatePostId(postId);
            if (!(postType == "Post" || postType == "Reel"))
            {
                errors.Add(new ValidationError
                {
                    message = CustomErrorMessage.InvalidPostType,
                    reference = "typepost",
                    parameter = "typepost",
                    errorCode = CustomErrorCode.InvalidPostType
                });
            }
            return errors;
        }
        public List<ValidationError> ValidateDeletePostId(long postId)
        {
            ValidatePostId(postId);
            return errors;
        }
        public List<ValidationError> ValidateLikePost(long userId, long postId)
        {
            ValidateUserId(userId);
            ValidatePostId(postId);
            return errors;
        }
        public List<ValidationError> ValidateCommentPost(CommentPostDTO model)
        {
            ValidateUserId(model.UserId);
            ValidatePostId(model.PostId);
            if (string.IsNullOrWhiteSpace(model.CommentText))
            {
                errors.Add(new ValidationError
                {
                    message = CustomErrorMessage.CommentRequired,
                    reference = "comment",
                    parameter = "comment",
                    errorCode = CustomErrorCode.NullComment
                });
            }
            return errors;
        }
        public List<ValidationError> ValidateCommentId(long commentId)
        {
            if (commentId == 0)
            {
                errors.Add(new ValidationError
                {
                    message = CustomErrorMessage.NullCommentId,
                    reference = "commentId",
                    parameter = "commentId",
                    errorCode = CustomErrorCode.NullCommentId
                });
            }
            else if (commentId < 0)
            {
                errors.Add(new ValidationError
                {
                    message = CustomErrorMessage.InvalidCommentId,
                    reference = "commentId",
                    parameter = "commentId",
                    errorCode = CustomErrorCode.InvalidCommentId
                });
            }
            if (!_dbcontext.Comments.Any(m => m.CommentId == commentId && m.IsDeleted != true))
            {
                errors.Add(new ValidationError
                {
                    message = CustomErrorMessage.ExitsPOstComment,
                    reference = "commentId",
                    parameter = "commentId",
                    errorCode = CustomErrorCode.IsNotComment
                });
            }
            return errors;
        }
        public List<ValidationError> ValidateStoryFile(AddStoryDTO model)
        {
            if (model.Story == null)
            {
                errors.Add(new ValidationError
                {
                    message = CustomErrorMessage.NullStoryPhoto,
                    reference = "Story",
                    parameter = "Story",
                    errorCode = CustomErrorCode.NullProfilePhoto
                });
            }
            else
            {
                string fileExtension = Path.GetExtension(model.Story.FileName).ToLowerInvariant();
                if (!AllowedExtensions.Contains(fileExtension))
                {
                    errors.Add(new ValidationError
                    {
                        message = string.Format(CustomErrorMessage.InvalidPhotoExtension, string.Join(", ", AllowedExtensions)),
                        reference = "Story",
                        parameter = "Story",
                        errorCode = CustomErrorCode.InvalidPhotoExtension
                    });
                }
                if (model.Story.ContentType.Contains("image"))
                {
                    // Photo file size limit (1 MB)
                    if (model.Story.Length > 1 * 1024 * 1024) // 1 MB in bytes
                    {
                        errors.Add(new ValidationError
                        {
                            message = CustomErrorMessage.FileSizeLimitExceeded,
                            reference = "Files",
                            parameter = "Files",
                            errorCode = CustomErrorCode.FileSizeLimitExceeded
                        });
                    }
                }
                else if (model.Story.ContentType.Contains("video"))
                {
                    // Video file size limit (3 MB less than)
                    if (model.Story.Length > 3 * 1024 * 1024) // 3 MB in bytes
                    {
                        errors.Add(new ValidationError
                        {
                            message = CustomErrorMessage.VideoFileSizeLimitExceeded,
                            reference = "Files",
                            parameter = "Files",
                            errorCode = CustomErrorCode.FileSizeLimitExceeded
                        });
                    }
                }
            }
            return errors;
        }
        public List<ValidationError> ValidateStoryId(long storyId)
        {
            if (storyId == 0)
            {
                errors.Add(new ValidationError
                {
                    message = CustomErrorMessage.NullStoryId,
                    reference = "postid",
                    parameter = "postid",
                    errorCode = CustomErrorCode.NullStoryId
                });
            }
            else if (storyId < 0)
            {
                errors.Add(new ValidationError
                {
                    message = CustomErrorMessage.InvalidStoryId,
                    reference = "postid",
                    parameter = "postid",
                    errorCode = CustomErrorCode.InvalidStoryId
                });
            }
            else if (!_dbcontext.Stories.Any(m => m.StoryId == storyId && m.IsDeleted != true))
            {
                errors.Add(new ValidationError
                {
                    message = CustomErrorMessage.ExitsStory,
                    reference = "postid",
                    parameter = "postid",
                    errorCode = CustomErrorCode.IsNotStory
                });
            }
            return errors;
        }
        public List<ValidationError> ValidateGetStoryById(long userId, long storyId)
        {
            ValidateUserId(userId);
            ValidateStoryId(storyId);
            return errors;
        }
        public List<ValidationError> ValidateUpsertHighLight(HighLightRequestDTO model)
        {
            if (model.HighlightId > 0)
            {
                ValidateHighLightId(model.HighlightId);
            }
            if (string.IsNullOrWhiteSpace(model.HighlightName))
            {
                errors.Add(new ValidationError
                {
                    message = CustomErrorMessage.NullHighLightName,
                    reference = "highLightId",
                    parameter = "highLightId",
                    errorCode = CustomErrorCode.NullHighLightName
                });
            }
            return errors;
        }
        public List<ValidationError> ValidateUpsertCollection(CollectionRequestDTO model)
        {
            if (model.CollectionId > 0)
            {
                ValidateHighLightId(model.CollectionId);
            }
            if (string.IsNullOrWhiteSpace(model.CollectionName))
            {
                errors.Add(new ValidationError
                {
                    message = CustomErrorMessage.NullcollectionName,
                    reference = "highLightId",
                    parameter = "highLightId",
                    errorCode = CustomErrorCode.NullCollectionName
                });
            }
            return errors;
        }
        public List<ValidationError> ValidateAddStoryhighlight(long highLightId, long storyId, long userId)
        {
            ValidateHighLightId(highLightId);
            if (storyId == 0)
            {
                errors.Add(new ValidationError
                {
                    message = CustomErrorMessage.NullStoryId,
                    reference = "postid",
                    parameter = "postid",
                    errorCode = CustomErrorCode.NullStoryId
                });
            }
            else if (storyId < 0)
            {
                errors.Add(new ValidationError
                {
                    message = CustomErrorMessage.InvalidStoryId,
                    reference = "postid",
                    parameter = "postid",
                    errorCode = CustomErrorCode.InvalidStoryId
                });
            }
            else if (!_dbcontext.Stories.Any(m => m.StoryId == storyId && m.UserId == userId && m.IsDeleted != true))
            {
                errors.Add(new ValidationError
                {
                    message = CustomErrorMessage.ExitsStory,
                    reference = "postid",
                    parameter = "postid",
                    errorCode = CustomErrorCode.IsNotStory
                });
            }
            return errors;
        }
        public List<ValidationError> ValidateAddPostCollection(long collectionId, long postId, long userId)
        {
            ValidateColletionId(collectionId);
            ValidatePostId(postId);
           
            return errors;
        }
        public List<ValidationError> ValidateNotificationIds(List<long> notificationIds)
        {
            if (notificationIds == null || !notificationIds.Any())
            {
                errors.Add(new ValidationError
                {
                    message = CustomErrorMessage.NullNotificationId,
                    reference = "notificationIds",
                    parameter = "notificationIds",
                    errorCode = CustomErrorCode.NullNotificationId
                });
                return errors;
            }

            foreach (var id in notificationIds)
            {
                bool exists = _dbcontext.Notifications.Any(n => n.NotificationId == id);
                if (!exists)
                {
                    errors.Add(new ValidationError
                    {
                        message = CustomErrorMessage.ExitsNotification,
                        reference = "notificationId",
                        parameter = "notificationId",
                        errorCode = CustomErrorCode.ExitsNotification
                    });
                    return errors;
                }
            }

            return errors;
        }
    }
}
