using InstagramWebAPI.Common;
using InstagramWebAPI.DTO;
using InstagramWebAPI.Helpers;
using InstagramWebAPI.Interface;
using InstagramWebAPI.Utils;
using Microsoft.AspNetCore.Mvc;

namespace InstagramWebAPI.Controllers
{
    [Route("api/User")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly IValidationService _validationService;
        private readonly IUserService _userService;
        private readonly ResponseHandler _responseHandler;
        private readonly Helper _helper;
        private readonly IAuthService _authService;

        public UserController(IValidationService validationService, ResponseHandler responseHandler, IUserService userService, IAuthService authService)
        {
            _validationService = validationService;
            _responseHandler = responseHandler;
            _userService = userService;
            _authService = authService;
            this._helper = new Helper();
        }
        [HttpPost("UploadProfilePhoto")]
        public async Task<ActionResult<ResponseModel>> UploadProfilePhotoAsync([FromForm] UploadProfilePhotoDTO model)
        {
            try
            {
                List<ValidationError> errors = _validationService.ValidateProfileFile(model);
                if (errors.Any())
                {
                    return BadRequest(_responseHandler.BadRequest(CustomErrorCode.IsValid, CustomErrorMessage.ValidationProfile, errors));
                }

                ProfilePhotoResponseDTO profilePhotoDTO = await _userService.UploadProfilePhotoAsync(model);
                if (profilePhotoDTO == null)
                {
                    return BadRequest(_responseHandler.BadRequest(CustomErrorCode.IsUpload, CustomErrorMessage.UploadError, model));
                }
                return Ok(_responseHandler.Success(CustomErrorMessage.UploadPhoto, model));
            }
            catch (CustomException ex)
            {
                return NotFound(_responseHandler.NotFoundRequest(CustomErrorCode.IsNotExits, ex.Message, model));
            }
            catch (Exception ex)
            {
                return BadRequest(_responseHandler.BadRequest(CustomErrorCode.IsUpload, ex.Message, model));
            }
        }
        [HttpPut("UpdateProfile")]
        public async Task<ActionResult<ResponseModel>> UpdateProfileAsync(UserDTO model)
        {
            try
            {
                List<ValidationError> errors = _validationService.ValidateProfileData(model);
                if (errors.Any())
                {
                    return BadRequest(_responseHandler.BadRequest(CustomErrorCode.IsValid, CustomErrorMessage.ValidationUpdateProfile, errors));
                }

                if (await _authService.IsUniqueUserName(model.UserName ?? string.Empty))
                {
                    return BadRequest(_responseHandler.BadRequest(CustomErrorCode.IsUserName, CustomErrorMessage.DuplicateUsername, model));
                }
                UserDTO? user = await _userService.UpdateProfileAsync(model);
                if (user == null)
                {
                    return BadRequest(_responseHandler.BadRequest(CustomErrorCode.IsUpdate, CustomErrorMessage.UpdateProfile, model));
                }
                return Ok(_responseHandler.Success(CustomErrorMessage.UpdateProfileSuccess, user));
            }
            catch (CustomException ex)
            {
                return NotFound(_responseHandler.NotFoundRequest(CustomErrorCode.IsNotExits, ex.Message, model));
            }
            catch (Exception ex)
            {
                return BadRequest(_responseHandler.BadRequest(CustomErrorCode.IsUpdate, ex.Message, model));
            }
        }

        //[HttpPost]
        //public async Task<ActionResult<ResponseModel>> FollowRequestAsync(FollowRequestDTO model)
        //{
        //    try
        //    {
        //        List<ValidationError> errors = _validationService.ValidateFollowRequest(model);
        //        if (errors.Any())
        //        {
        //            return BadRequest(_responseHandler.BadRequest(CustomErrorCode.IsValid, CustomErrorMessage.ValidationRequest, errors));
        //        }

        //    }
        //    catch (CustomException ex)
        //    {
        //        return NotFound(_responseHandler.NotFoundRequest(CustomErrorCode.IsNotExits, ex.Message, model));
        //    }
        //    catch (Exception ex)
        //    {
        //        return BadRequest(_responseHandler.BadRequest(CustomErrorCode.IsUpdate, ex.Message, model));
        //    }
        //}
    }
}
