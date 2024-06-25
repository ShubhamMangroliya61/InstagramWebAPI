using InstagramWebAPI.Common;
using InstagramWebAPI.DAL.Models;
using InstagramWebAPI.DTO;
using InstagramWebAPI.Interface;
using InstagramWebAPI.Utils;
using Microsoft.AspNetCore.Mvc;
using System.Reflection.Metadata;
using System.Text.RegularExpressions;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace InstagramWebAPI.BLL
{
    public class ValidationService : IValidationService
    {
        private static readonly string[] AllowedExtensions = { ".jpg", ".jpeg", ".png"};

        /// <summary>
        /// Validates the registration request DTO.
        /// </summary>
        /// <param name="model">The <see cref="RegistrationRequestDTO"/> containing registration information.</param>
        /// <returns>A list of <see cref="ValidationError"/> objects indicating validation errors.</returns>
        public List<ValidationError> ValidateRegistration(RegistrationRequestDTO model)
        {
            List<ValidationError> errors = new();

            const string EmailRegex = @"^[\w\-\.]+@([\w-]+\.)+[\w-]{2,4}$";
            const string MobileRegex = @"^[6-9]{1}[0-9]{9}$";
            const string PasswordRegex = @"^(?=.*[A-Z])(?=.*\d)(?=.*[a-z])(?=.*\W).{7,15}$";
            const string UserNameRegex = @"^[a-zA-Z0-9][a-zA-Z0-9_.]{7,17}$";

            if (model == null)
            {
                errors.Add(new ValidationError
                {
                    message = CustomErrorMessage.ModelIsNull,
                    reference = "RegistrationRequestDTO",
                    parameter = "model",
                    errorCode = CustomErrorCode.ModelIsNull
                });
            }
            else
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

                if (string.IsNullOrWhiteSpace(model.Password))
                {
                    errors.Add(new ValidationError
                    {
                        message = CustomErrorMessage.PasswordRequired,
                        reference = "Password",
                        parameter = "Password",
                        errorCode = CustomErrorCode.NullPassword
                    });
                }
                else if (!Regex.IsMatch(model.Password, PasswordRegex))
                {
                    errors.Add(new ValidationError
                    {
                        message = CustomErrorMessage.InvalidPasswordFormat,
                        reference = "Password",
                        parameter = "Password",
                        errorCode = CustomErrorCode.InvalidPasswordFormat
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
            List<ValidationError> errors = new();

            const string EmailRegex = @"^[\w\-\.]+@([\w-]+\.)+[\w-]{2,4}$";
            const string MobileRegex = @"^[6-9]{1}[0-9]{9}$";
            const string PasswordRegex = @"^(?=.*[A-Z])(?=.*\d)(?=.*[a-z])(?=.*\W).{7,15}$";
            const string UserNameRegex = @"^[a-zA-Z0-9][a-zA-Z0-9_.]{7,17}$";


            if (model == null)
            {
                errors.Add(new ValidationError
                {
                    message = CustomErrorMessage.ModelIsNull,
                    reference = "LoginRequestDTO",
                    parameter = "model",
                    errorCode = CustomErrorCode.ModelIsNull
                });
            }
            else
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
                    else if(model.TypeUserId == "username" && !Regex.IsMatch(model.UserID, UserNameRegex))
                    {
                        errors.Add(new ValidationError
                        {
                            message = CustomErrorMessage.InvalidUserNameFormat,
                            reference = "Password",
                            parameter = "Password",
                            errorCode = CustomErrorCode.InvalidUserNameFormat
                        });
                    }

                    if (string.IsNullOrWhiteSpace(model.Password))
                    {
                        errors.Add(new ValidationError
                        {
                            message = CustomErrorMessage.PasswordRequired,
                            reference = "Password",
                            parameter = "Password",
                            errorCode = CustomErrorCode.NullPassword
                        });
                    }
                    else if (!Regex.IsMatch(model.Password, PasswordRegex))
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
            }
            return errors;
        }

        /// <summary>
        /// Validates the reset password request DTO.
        /// </summary>
        /// <param name="model">The reset password request DTO containing email or mobile number.</param>
        /// <returns>A list of validation errors, if any.</returns>
        public List<ValidationError> ValidateResetPassword(ResetPasswordDTO model)
        {
            const string EmailRegex = @"^[\w\-\.]+@([\w-]+\.)+[\w-]{2,4}$";
            const string MobileRegex = @"^[6-9]{1}[0-9]{9}$";
            List<ValidationError> errors = new();

            if (!string.IsNullOrWhiteSpace(model.Email) && !Regex.IsMatch(model.Email, EmailRegex))
            {
                errors.Add(new ValidationError
                {
                    message = CustomErrorMessage.InvalidEmailFormat,
                    reference = "Email",
                    parameter = model.Email,
                    errorCode = CustomErrorCode.InvalidEmailFormat
                });
            }
            else if (!string.IsNullOrWhiteSpace(model.MobileNumber) && !Regex.IsMatch(model.MobileNumber, MobileRegex))
            {
                errors.Add(new ValidationError
                {
                    message = CustomErrorMessage.InvalidMobileNumberFormat,
                    reference = "MobileNumber",
                    parameter = model.MobileNumber,
                    errorCode = CustomErrorCode.InvalidMobileNumberFormat
                });
            }
            return errors;
        }

        /// <summary>
        /// Validates the reset password data including password and confirm password fields.
        /// </summary>
        /// <param name="model">The reset password DTO containing password and confirm password fields.</param>
        /// <returns>A list of validation errors, if any.</returns>
        public List<ValidationError> ValidateResetPasswordData(ResetPasswordDTO model)
        {
            const string PasswordRegex = @"^(?=.*[A-Z])(?=.*\d)(?=.*[a-z])(?=.*\W).{7,15}$";
            List<ValidationError> errors = new();

            if (string.IsNullOrWhiteSpace(model.Password))
            {
                errors.Add(new ValidationError
                {
                    message = CustomErrorMessage.PasswordRequired,
                    reference = "Password",
                    parameter = "Password",
                    errorCode = CustomErrorCode.NullPassword
                });
            }
            else if (!Regex.IsMatch(model.Password, PasswordRegex))
            {
                errors.Add(new ValidationError
                {
                    message = CustomErrorMessage.InvalidPasswordFormat,
                    reference = "Password",
                    parameter = "Password",
                    errorCode = CustomErrorCode.InvalidPasswordFormat
                });
            }

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
            List<ValidationError> errors = new ();

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
                if (!AllowedExtensions.Contains(fileExtension))
                {
                    errors.Add(new ValidationError
                    {
                        message = string.Format(CustomErrorMessage.InvalidPhotoExtension, string.Join(", ", AllowedExtensions)),
                        reference = "ProfilePhoto",
                        parameter = "ProfilePhoto",
                        errorCode = CustomErrorCode.InvalidPhotoExtension
                    });
                }
            }
            return errors;
        }

        public List<ValidationError> ValidateUserId(long userId)
        {
            List<ValidationError> errors = new();

            if (userId <= 0)
            {
                errors.Add(new ValidationError
                {
                    message = CustomErrorMessage.InvalidUserId,
                    reference = "UserId",
                    parameter = "UserId",
                    errorCode = CustomErrorCode.InvalidUserId
                });
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
            List<ValidationError> errors = new();
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
    }
}
