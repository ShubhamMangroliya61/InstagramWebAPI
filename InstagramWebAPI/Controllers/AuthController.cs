using InstagramWebAPI.Common;
using InstagramWebAPI.Helpers;
using InstagramWebAPI.DAL.Models;
using InstagramWebAPI.DTO;
using InstagramWebAPI.Interface;
using InstagramWebAPI.Utils;
using Microsoft.AspNetCore.Mvc;
using static System.Runtime.InteropServices.JavaScript.JSType;
using Microsoft.AspNetCore.Authorization;

namespace InstagramWebAPI.Controllers
{
    [Route("api/Auth")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IValidationService _validationService;
        private readonly IAuthService _authService;
        private readonly ResponseHandler _responseHandler;
        private readonly Helper _helper;

        public AuthController(IValidationService validationService, ResponseHandler responseHandler, IAuthService authService,Helper helper)
        {
            _validationService = validationService;
            _responseHandler = responseHandler;
            _authService = authService;
            _helper = helper;
        }

        /// <summary>
        /// Registers a new user asynchronously.
        /// </summary>
        /// <param name="model">The registration information.</param>
        /// <returns>A response indicating success or failure of the registration.</returns>
        /// <response code="200">Returns when the registration is successful.</response>
        /// <response code="400">Returns when there are validation errors or registration fails.</response>
        [HttpPost("Register")]
        public async Task<ActionResult> UserRegisterAsync([FromBody] UserDTO model)
        {
            try
            {
                List<ValidationError> errors = _validationService.ValidateRegistration(model);
                if (errors.Any())
                {
                    return BadRequest(_responseHandler.BadRequest(CustomErrorCode.IsValid, CustomErrorMessage.ValidationRegistrtion, errors));
                }

                UserDTO? user = await _authService.UpSertUserAsync(model);
                if (user == null)
                {
                    return BadRequest(_responseHandler.BadRequest(CustomErrorCode.IsRegister, CustomErrorMessage.RegistrationError,""));
                }

                return Ok(_responseHandler.Success(CustomErrorMessage.RegistrationSucces, user));
            }
            catch (Exception ex)
            {
                return BadRequest(_responseHandler.BadRequest(CustomErrorCode.IsRegister, ex.Message, ""));
            }
        }

        /// <summary>
        /// Logs in a user asynchronously.
        /// </summary>
        /// <param name="model">The login credentials.</param>
        /// <returns>An IActionResult representing the success or failure of the login attempt.</returns>
        /// <response code="200">Returns when the login is successful.</response>
        /// <response code="400">Returns when there are validation errors or login fails.</response>
        [HttpPost("Login")]
        public async Task<ActionResult> UserLoginAsync([FromBody] LoginRequestDTO model)
        {
            try
            {
                List<ValidationError> errors = _validationService.ValidateLogin(model);
                if (errors.Any())
                {
                    return BadRequest(_responseHandler.BadRequest(CustomErrorCode.IsValid, CustomErrorMessage.ValidationLogin, errors));
                }

                LoginResponseDTO loginResponse = await _authService.UserLoginAsync(model);
                if (string.IsNullOrEmpty(loginResponse.Token))
                {
                    return BadRequest(_responseHandler.BadRequest(CustomErrorCode.IsLogin, CustomErrorMessage.InvalidUsernameOrPassword, ""));
                }

                return Ok(_responseHandler.Success(CustomErrorMessage.LoginSucces, loginResponse));
            }
            catch (Exception ex)
            {
                return BadRequest(_responseHandler.BadRequest(CustomErrorCode.LoginError, ex.Message, ""));
            }
        }

        /// <summary>
        /// Resets the password for a user asynchronously.
        /// </summary>
        /// <param name="model">The model containing the email address of the user requesting password reset.</param>
        /// <returns>An <see cref="ActionResult{T}"/> representing the result of the password reset request.</returns>
        /// <response code="200">Returns when the password reset email is successfully sent.</response>
        /// <response code="400">Returns when there are validation errors, the email fails to send, or other errors occur.</response>
        [HttpPost("ForgotPassword")]
        public async Task<ActionResult> ForgotPasswordAsync([FromBody] ForgotPasswordDTO model)
        {
            try
            {
                List<ValidationError> errors = _validationService.ValidateForgotPassword(model);
                if (errors.Any())
                {
                    return BadRequest(_responseHandler.BadRequest(CustomErrorCode.InvalidEmailFormat, CustomErrorMessage.InvalidEmailFormat, errors));
                }

                User user = await _authService.GetUser(model);

                byte[] b = System.Text.ASCIIEncoding.ASCII.GetBytes(user.UserId.ToString());
                string encryptedUserId = Convert.ToBase64String(b);

                string subject = "Forgot Password - Instagram";
                string resetLink = $"https://e828-202-131-123-10.ngrok-free.app/resetpassword/{encryptedUserId}";
                
                string htmlMessage = $@"
                                    <html>
                        <body style=""font-family: Arial, sans-serif; background-color:rgb(243, 242, 242);  padding: 20px;"">
 
                            <!-- Header -->
                            <div style="" padding: 10px; text-align: center;"">
                                <img src=""https://encrypted-tbn0.gstatic.com/images?q=tbn:ANd9GcRNFFDufYJlSyMP1NgyV8OUR_zYH9YIcCcCUA&s""  style=""width: 120px; height: auto;"">
                            </div>
                            <div style="" padding-left: 15px; border-radius: 5px; margin-top: 20px;"">
                                <p>Hi <span style=""color: #0095f6;"">{user.Name}</span>,</p>
                                <p>Sorry to hear you’re having trouble logging into Instagram. We got a message that you forgot your password. If this was you, you can get right back into your account or reset your password now.</p>
                                <div style=""text-align: center; margin-top: 20px;"">
                                    <br>
                                    <a href=""{resetLink}"" style=""display: inline-block; background-color: #0095f6; color: white; text-decoration: none; padding: 10px 20px; border-radius: 5px;"">Reset your password</a>
                                </div>
                                <p style=""margin-top: 20px;"">If you didn’t request a login link or a password reset, you can ignore this message and <a href=""#"" style=""color: #0095f6; text-decoration: none;"">learn more about why you may have received it.</a></p>
                                <p>Only people who know your Instagram password or click the login link in this email can log into your account.</p>
                            </div>
                        </body>
                        </html>
                        ";

                // Send email using EmailSender method
                if (!await _helper.EmailSender(user.Email??string.Empty, subject, htmlMessage))
                {
                    return BadRequest(_responseHandler.BadRequest(CustomErrorCode.MailNotSend, CustomErrorMessage.MailNotSend, ""));
                }
                return Ok(_responseHandler.Success(CustomErrorMessage.MailSend, model));

            }
            catch (Exception ex)
            {
                return BadRequest(_responseHandler.BadRequest(CustomErrorCode.IsNotExits, ex.Message, ""));
            }
        }

        /// <summary>
        /// Handles the data for resetting password asynchronously.
        /// </summary>
        /// <param name="model">The model containing reset password data.</param>
        /// <returns>An <see cref="ActionResult{T}"/> representing the result of the password reset operation.</returns>
        /// <response code="200">Returns when the password is successfully reset.</response>
        /// <response code="400">Returns when there are validation errors or the password reset operation fails.</response>
        [HttpPost("ForgotPasswordUpdate")]
        public async Task<ActionResult> ForgotPasswordUpdateAsync([FromBody] ForgotPasswordDTO model)
        {
            try
            {
                byte[] b = Convert.FromBase64String(model.EncyptUserId??string.Empty.ToString());
                string dcryptedUserId = System.Text.ASCIIEncoding.ASCII.GetString(b);
                model.UserId = Convert.ToInt32(dcryptedUserId);

                List<ValidationError> errors = _validationService.ValidateForgotPasswordData(model);
                if (errors.Any())
                {
                    return BadRequest(_responseHandler.BadRequest(CustomErrorCode.IsValid, CustomErrorMessage.ValidationReset, errors));
                }

                bool IsData = await _authService.ForgotPasswordAsync(model);

                if (!IsData)
                {
                    return BadRequest(_responseHandler.BadRequest(CustomErrorCode.IsReset, CustomErrorMessage.ReserError, ""));
                }
                return Ok(_responseHandler.Success(CustomErrorMessage.ForgotPassword, model));
            }
            catch
            {
                return BadRequest(_responseHandler.BadRequest(CustomErrorCode.IsReset, CustomErrorMessage.ReserError, ""));
            }
        }

        /// <summary>
        /// Handles the data for resetting password asynchronously.
        /// </summary>
        /// <param name="model">The model containing reset password data.</param>
        /// <returns>An <see cref="ActionResult{T}"/> representing the result of the password reset operation.</returns>
        [HttpPost("ResetPasswordAsync")]
        [Authorize]
        public async Task<ActionResult> ResetPasswordAsync([FromBody] ResetPasswordRequestDTO model)
        {
            try
            {
                List<ValidationError> errors = _validationService.ValidateResetPassword(model);
                if (errors.Any())
                {
                    return BadRequest(_responseHandler.BadRequest(CustomErrorCode.IsValid, CustomErrorMessage.ValidationReset, errors));
                }
                bool IsData = await _authService.ResetPasswordAsync(model);

                if (!IsData)
                {
                    return BadRequest(_responseHandler.BadRequest(CustomErrorCode.IsReset, CustomErrorMessage.ReserError, ""));
                }
                return Ok(_responseHandler.Success(CustomErrorMessage.ReserPassword, model));
            }
            catch
            {
                return BadRequest(_responseHandler.BadRequest(CustomErrorCode.IsReset, CustomErrorMessage.ReserError, ""));
            }
        }
    }
}
