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

        public void ValidateEmail(string email)
        {
            if (string.IsNullOrWhiteSpace(email))
            {
                errors.Add(new ValidationError
                {
                    message = CustomErrorMessage.UsernameRequired,
                    reference = "UserName",
                    parameter = "UserName",
                    errorCode = CustomErrorCode.NullEmail
                });
            }
            else if (!Regex.IsMatch(email, EmailRegex))
            {
                errors.Add(new ValidationError
                {
                    message = CustomErrorMessage.InvalidEmailFormat,
                    reference = "Password",
                    parameter = "Password",
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
                if (!DateTime.TryParseExact(dateOfBirth, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime date))
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
                    reference = "Password",
                    parameter = "Password",
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
            if (string.IsNullOrWhiteSpace(contactNumber))
            {
                errors.Add(new ValidationError
                {
                    message = CustomErrorMessage.UsernameRequired,
                    reference = "UserName",
                    parameter = "UserName",
                    errorCode = CustomErrorCode.NullContactNumber
                });
            }
            else if (!Regex.IsMatch(contactNumber, MobileRegex))
            {
                errors.Add(new ValidationError
                {
                    message = CustomErrorMessage.InvalidMobileNumberFormat,
                    reference = "Password",
                    parameter = "Password",
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
            else if (!_dbcontext.Users.Any(m => m.UserId == userId && m.IsDeleted != true))
            {
                errors.Add(new ValidationError
                {
                    message = CustomErrorMessage.ExitsUser,
                    reference = "UserName",
                    parameter = "UserName",
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
                    reference = "UserId",
                    parameter = "UserId",
                    errorCode = CustomErrorCode.NullRequestId
                });
            }
            else if (requestId < 0)
            {
                errors.Add(new ValidationError
                {
                    message = CustomErrorMessage.InvalidRequestId,
                    reference = "UserId",
                    parameter = "UserId",
                    errorCode = CustomErrorCode.InvalidRequestId
                });
            }
            else if (!_dbcontext.Requests.Any(m => m.RequestId == requestId && m.IsDeleted != true))
            {
                errors.Add(new ValidationError
                {
                    message = CustomErrorMessage.ExitsRequest,
                    reference = "UserName",
                    parameter = "UserName",
                    errorCode = CustomErrorCode.IsNotRequest
                });
            }
            return errors;
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
                if (string.IsNullOrWhiteSpace(model.EmailOrNumber))
                {
                    errors.Add(new ValidationError
                    {
                        message = CustomErrorMessage.EmailOrMobileRequired,
                        reference = "Email OR MobileNumber",
                        parameter = "Email or MobileNumber",
                        errorCode = CustomErrorCode.NullEmailOrMobileNumber
                    });
                }
                else
                {
                    if (model.Type == "email" && !Regex.IsMatch(model.EmailOrNumber, EmailRegex))
                    {
                        errors.Add(new ValidationError
                        {
                            message = CustomErrorMessage.InvalidEmailFormat,
                            reference = "Email",
                            parameter = model.EmailOrNumber,
                            errorCode = CustomErrorCode.InvalidEmailFormat
                        });
                    }
                    else if (model.Type == "phone" && !Regex.IsMatch(model.EmailOrNumber, MobileRegex))
                    {
                        errors.Add(new ValidationError
                        {
                            message = CustomErrorMessage.InvalidMobileNumberFormat,
                            reference = "MobileNumber",
                            parameter = model.EmailOrNumber,
                            errorCode = CustomErrorCode.InvalidMobileNumberFormat
                        });
                    }
                }

                ValidatePassword(model.Password ?? string.Empty);
            }
            ValidateDateOfBirth(model.DateOfBirth ?? string.Empty);
            ValidateUserName(model.UserName);

            if (model.UserId > 0)
            {
                ValidateUserId(model.UserId);
                ValidateEmail(model.Email ?? string.Empty);
                ValidateContactNumber(model.ContactNumber ?? string.Empty);
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
                if (model.TypeUserId == "email" && !Regex.IsMatch(model.UserID, EmailRegex))
                {
                    errors.Add(new ValidationError
                    {
                        message = CustomErrorMessage.InvalidEmailFormat,
                        reference = "Email",
                        parameter = model.UserID,
                        errorCode = CustomErrorCode.InvalidEmailFormat
                    });
                }
                else if (model.TypeUserId == "phone" && !Regex.IsMatch(model.UserID, MobileRegex))
                {
                    errors.Add(new ValidationError
                    {
                        message = CustomErrorMessage.InvalidMobileNumberFormat,
                        reference = "MobileNumber",
                        parameter = model.UserID,
                        errorCode = CustomErrorCode.InvalidMobileNumberFormat
                    });
                }
                else if (model.TypeUserId == "username" && !Regex.IsMatch(model.UserID, UserNameRegex))
                {
                    errors.Add(new ValidationError
                    {
                        message = CustomErrorMessage.InvalidUserNameFormat,
                        reference = "Password",
                        parameter = "Password",
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
        public List<ValidationError> ValidateProfileFile(UploadProfilePhotoDTO model)
        {
            List<ValidationError> errors = new();

            ValidateUserId(model.UserId);

            if (model.ProfilePhoto == null)
            {
                errors.Add(new ValidationError
                {
                    message = CustomErrorMessage.NullProfilePhoto,
                    reference = "ProfilePhoto",
                    parameter = "ProfilePhoto",
                    errorCode = CustomErrorCode.NullProfilePhoto
                });
            }
            else
            {
                string fileExtension = Path.GetExtension(model.ProfilePhoto.FileName).ToLowerInvariant();
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
                    reference = "Password",
                    parameter = "Password",
                    errorCode = CustomErrorCode.InvalidUserNameFormat
                });
            }
            return errors;
        }
        public List<ValidationError> ValidateFollowRequest(FollowRequestDTO model)
        {
            if (model.FromUserId == model.ToUserId)
            {
                errors.Add(new ValidationError
                {
                    message = CustomErrorMessage.MatchUserId,
                    reference = "UserId",
                    parameter = "UserId",
                    errorCode = CustomErrorCode.SameUserId
                });
            }
            if (model.FromUserId <= 0)
            {
                errors.Add(new ValidationError
                {
                    message = CustomErrorMessage.InvalidUserId,
                    reference = "UserId",
                    parameter = "UserId",
                    errorCode = CustomErrorCode.InvalidFromUserId
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
                    reference = "UserId",
                    parameter = "UserId",
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
        public List<ValidationError> ValidateGetUserById(long userid)
        {
            ValidateUserId(userid);
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
                    reference = "UserId",
                    parameter = "UserId",
                    errorCode = CustomErrorCode.InvalidReqType
                });
            }
            return errors;
        }

        public List<ValidationError> ValidateCreatePost(CreatePostDTO model)
        {
            ValidateUserId(model.UserId);

            if (model.File == null || model.File.Count == 0)
            {
                errors.Add(new ValidationError
                {
                    message = CustomErrorMessage.NullProfilePhoto,
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
                }
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
            if(!BCrypt.Net.BCrypt.Verify(model.OldPassword, user.Password))
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
    }
}
