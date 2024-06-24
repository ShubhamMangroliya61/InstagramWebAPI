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
                if (string.IsNullOrWhiteSpace(model.Email) && string.IsNullOrWhiteSpace(model.MobileNumber))
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
                    if (!string.IsNullOrWhiteSpace(model.MobileNumber) && !Regex.IsMatch(model.MobileNumber, MobileRegex))
                    {
                        errors.Add(new ValidationError
                        {
                            message = CustomErrorMessage.InvalidMobileNumberFormat,
                            reference = "MobileNumber",
                            parameter = model.MobileNumber,
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
                if (string.IsNullOrWhiteSpace(model.Email) && string.IsNullOrWhiteSpace(model.MobileNumber) && string.IsNullOrWhiteSpace(model.UserName))
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
    }
}
