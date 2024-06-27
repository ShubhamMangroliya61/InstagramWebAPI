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

        public AuthController(IValidationService validationService, ResponseHandler responseHandler, IAuthService authService)
        {
            _validationService = validationService;
            _responseHandler = responseHandler;
            _authService = authService;
            this._helper = new Helper();
        }

        /// <summary>
        /// Registers a new user asynchronously.
        /// </summary>
        /// <param name="model">The registration information.</param>
        /// <returns>A response indicating success or failure of the registration.</returns>
        /// <response code="200">Returns when the registration is successful.</response>
        /// <response code="400">Returns when there are validation errors or registration fails.</response>
        [HttpPost("Register")]
        public async Task<ActionResult<ResponseModel>> UserRegisterAsync([FromBody] UserDTO model)
        {
            try
            {
                List<ValidationError> errors = _validationService.ValidateRegistration(model);
                if (errors.Any())
                {
                    return BadRequest(_responseHandler.BadRequest(CustomErrorCode.IsValid, CustomErrorMessage.ValidationRegistrtion, errors));
                }

                if (await _authService.IsUniqueUserNameEmailPhoneNumber(model))
                {
                    return BadRequest(_responseHandler.BadRequest(CustomErrorCode.IsUserName, CustomErrorMessage.DuplicateUsername, model));
                }

                User? user = await _authService.UpSertUserAsync(model);
                if (user == null)
                {
                    return BadRequest(_responseHandler.BadRequest(CustomErrorCode.IsRegister, CustomErrorMessage.RegistrationError, model));
                }

                return Ok(_responseHandler.Success(CustomErrorMessage.RegistrationSucces, user));
            }
            catch (Exception ex)
            {
                return BadRequest(_responseHandler.BadRequest(CustomErrorCode.IsRegister, ex.Message, model));
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
        public async Task<ActionResult<ResponseModel>> UserLoginAsync([FromBody] LoginRequestDTO model)
        {
            try
            {
                List<ValidationError> errors = _validationService.ValidateLogin(model);
                if (errors.Any())
                {
                    return BadRequest(_responseHandler.BadRequest(CustomErrorCode.IsValid, CustomErrorMessage.ValidationLogin, errors));
                }

                LoginResponseDTO loginResponse = await _authService.UserLoginAsync(model);
                if (loginResponse.User == null && string.IsNullOrEmpty(loginResponse.Token))
                {
                    return BadRequest(_responseHandler.BadRequest(CustomErrorCode.IsLogin, CustomErrorMessage.InvalidUsernameOrPassword, loginResponse));
                }

                return Ok(_responseHandler.Success(CustomErrorMessage.LoginSucces, loginResponse));
            }
            catch (Exception ex)
            {
                return BadRequest(_responseHandler.BadRequest(CustomErrorCode.LoginError, ex.Message, model));
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
        public async Task<ActionResult<ResponseModel>> ForgotPasswordAsync([FromBody] ForgotPasswordDTO model)
        {
            try
            {
                List<ValidationError> errors = _validationService.ValidateForgotPassword(model);
                if (errors.Any())
                {
                    return BadRequest(_responseHandler.BadRequest(CustomErrorCode.InvalidEmailFormat, CustomErrorMessage.InvalidEmailFormat, errors));
                }

                User user = await _authService.GetUser(model);

                var subject = "Forgot Password - Instagram";
                var message = "Tap on link for Forgot Password: ";

                if (!await _helper.EmailSender(user.Email ?? string.Empty, subject, message))
                {
                    return BadRequest(_responseHandler.BadRequest(CustomErrorCode.MailNotSend, CustomErrorMessage.MailNotSend, model));
                }
                return Ok(_responseHandler.Success(CustomErrorMessage.MailSend, model));

            }
            catch (Exception ex)
            {
                return BadRequest(_responseHandler.BadRequest(CustomErrorCode.IsNotExits, ex.Message, model));
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
        public async Task<ActionResult<ResponseModel>> ForgotPasswordUpdateAsync([FromBody] ForgotPasswordDTO model)
        {
            try
            {
                List<ValidationError> errors = _validationService.ValidateForgotPasswordData(model);
                if (errors.Any())
                {
                    return BadRequest(_responseHandler.BadRequest(CustomErrorCode.IsValid, CustomErrorMessage.ValidationReset, errors));
                }

                bool IsData = await _authService.ForgotPasswordAsync(model);

                if (!IsData)
                {
                    return BadRequest(_responseHandler.BadRequest(CustomErrorCode.IsReset, CustomErrorMessage.ReserError, model));
                }
                return Ok(_responseHandler.Success(CustomErrorMessage.ForgotPassword, model));
            }
            catch
            {
                return BadRequest(_responseHandler.BadRequest(CustomErrorCode.IsReset, CustomErrorMessage.ReserError, model));
            }
        }

        /// <summary>
        /// Handles the data for resetting password asynchronously.
        /// </summary>
        /// <param name="model">The model containing reset password data.</param>
        /// <returns>An <see cref="ActionResult{T}"/> representing the result of the password reset operation.</returns>
        [HttpPost("ResetPasswordAsync")]
        [Authorize]
        public async Task<ActionResult<ResponseModel>> ResetPasswordAsync([FromBody] ResetPasswordRequestDTO model)
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
                    return BadRequest(_responseHandler.BadRequest(CustomErrorCode.IsReset, CustomErrorMessage.ReserError, model));
                }
                return Ok(_responseHandler.Success(CustomErrorMessage.ReserPassword, model));
            }
            catch
            {
                return BadRequest(_responseHandler.BadRequest(CustomErrorCode.IsReset, CustomErrorMessage.ReserError, model));
            }
        }
    }
}
