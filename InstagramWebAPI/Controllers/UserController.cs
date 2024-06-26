using DataAccess.CustomModel;
using InstagramWebAPI.Common;
using InstagramWebAPI.DAL.Models;
using InstagramWebAPI.DTO;
using InstagramWebAPI.Helpers;
using InstagramWebAPI.Interface;
using InstagramWebAPI.Utils;
using Microsoft.AspNetCore.JsonPatch.Internal;
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
                return Ok(_responseHandler.Success(CustomErrorMessage.UploadPhoto, profilePhotoDTO));
            }
            catch (Exception ex)
            {
                if (ex is ValidationException vx)
                {
                    return BadRequest(_responseHandler.BadRequest(vx.ErrorCode, vx.Message, vx.Errors));
                }
                else
                {
                    return BadRequest(_responseHandler.BadRequest(CustomErrorCode.IsUpload, ex.Message, model));
                }
            }
        }
        [HttpPut("UpdateProfile")]
        public async Task<ActionResult<ResponseModel>> UpdateProfileAsync(UserDTO model)
        {
            try
            {
                List<ValidationError> errors = _validationService.ValidateRegistration(model);
                if (errors.Any())
                {
                    return BadRequest(_responseHandler.BadRequest(CustomErrorCode.IsValid, CustomErrorMessage.ValidationUpdateProfile, errors));
                }

                if (await _authService.IsUniqueUserName(model.UserName ?? string.Empty))
                {
                    return BadRequest(_responseHandler.BadRequest(CustomErrorCode.IsUserName, CustomErrorMessage.DuplicateUsername, model));
                }

                User? user = await _authService.UpSertUserAsync(model);
                if (user == null)
                {
                    return BadRequest(_responseHandler.BadRequest(CustomErrorCode.IsUpdate, CustomErrorMessage.UpdateProfile, model));
                }
                return Ok(_responseHandler.Success(CustomErrorMessage.UpdateProfileSuccess, user));
            }
            catch (Exception ex)
            {
                if (ex is ValidationException vx)
                {
                    return BadRequest(_responseHandler.BadRequest(vx.ErrorCode, vx.Message, vx.Errors));
                }
                else
                {
                    return BadRequest(_responseHandler.BadRequest(CustomErrorCode.IsUpload, ex.Message, model));
                }
            }
        }

        [HttpPost("FollowRequest")]
        public async Task<ActionResult<ResponseModel>> FollowRequestAsync(FollowRequestDTO model)
        {
            try
            {
                List<ValidationError> errors = _validationService.ValidateFollowRequest(model);
                if (errors.Any())
                {
                    return BadRequest(_responseHandler.BadRequest(CustomErrorCode.IsValid, CustomErrorMessage.ValidationRequest, errors));
                }
                bool isFollow =await _userService.FollowRequestAsync(model);
                if (!isFollow)
                {
                    return BadRequest(_responseHandler.BadRequest(CustomErrorCode.IsRequest, CustomErrorMessage.RequestError, errors));
                }
                return Ok(_responseHandler.Success(CustomErrorMessage.RequestSuccess, model));
            }
            catch (Exception ex)
            {
                if (ex is ValidationException vx)
                {
                    return BadRequest(_responseHandler.BadRequest(vx.ErrorCode, vx.Message, vx.Errors));
                }
                else
                {
                    return BadRequest(_responseHandler.BadRequest(CustomErrorCode.IsUpload, ex.Message, model));
                }
            }
        }

        [HttpGet("FollowerORFollowingListById")]
        public async Task<ActionResult<ResponseModel>> GetFollowerORFollowingListByIdAsync([FromQuery] RequestDTO<FollowerListRequestDTO> model)
        {
            try
            {
                List<ValidationError> errors = _validationService.ValidateFollowerList(model);
                if (errors.Any())
                {
                    return BadRequest(_responseHandler.BadRequest(CustomErrorCode.IsValid, CustomErrorMessage.ValidationRequest, errors));
                }
                PaginationResponceModel<UserDTO> data =await _userService.GetFollowerORFollowingListAsync(model);
                if (data == null)
                {
                    return BadRequest(_responseHandler.BadRequest(CustomErrorCode.IsGetLIst, CustomErrorMessage.GetFollowerList, model));
                }
                return Ok(_responseHandler.Success(CustomErrorMessage.GetFollowerListSucces, data));
            }
            catch (Exception ex)
            {
                if (ex is ValidationException vx)
                {
                    return BadRequest(_responseHandler.BadRequest(vx.ErrorCode, vx.Message, vx.Errors));
                }
                else
                {
                    return BadRequest(_responseHandler.BadRequest(CustomErrorCode.IsGetLIst, ex.Message, model));
                }
            }
        }

        [HttpGet("RequestListById")]
        public async Task<ActionResult<ResponseModel>> GetRequestListByIdAsync([FromQuery] RequestDTO<FollowRequestDTO> model)
        {
            try
            {
                List<ValidationError> errors = _validationService.ValidateRequestList(model);
                if (errors.Any())
                {
                    return BadRequest(_responseHandler.BadRequest(CustomErrorCode.IsValid, CustomErrorMessage.ValidationRequest, errors));
                }
                PaginationResponceModel<RequestListResponseDTO> data = await _userService.GetRequestListByIdAsync(model);
                if (data == null)
                {
                    return BadRequest(_responseHandler.BadRequest(CustomErrorCode.IsGetLIst, CustomErrorMessage.GetFollowerList, model));
                }
                return Ok(_responseHandler.Success(CustomErrorMessage.GetFollowerListSucces, data));
            }
            catch (Exception ex)
            {
                if (ex is ValidationException vx)
                {
                    return BadRequest(_responseHandler.BadRequest(vx.ErrorCode, vx.Message, vx.Errors));
                }
                else
                {
                    return BadRequest(_responseHandler.BadRequest(CustomErrorCode.IsGetLIst, ex.Message, model));
                }
            }
        }

        [HttpGet("GetUserById")]
        public async Task<ActionResult<ResponseModel>> GetUserById([FromQuery] long userId)
        {
            try
            {
                List<ValidationError> errors = _validationService.ValidateGetUserById(userId);
                if (errors.Any())
                {
                    return BadRequest(_responseHandler.BadRequest(CustomErrorCode.IsValid, CustomErrorMessage.ExitsUser, errors));
                }
                UserDTO data = await _userService.GetUserByIdAsync(userId);
                if (data == null)
                {
                    return BadRequest(_responseHandler.BadRequest(CustomErrorCode.IsNotExits, CustomErrorMessage.ExitsUser, userId));
                }
                return Ok(_responseHandler.Success(CustomErrorMessage.GetUser, data));
            }
            catch (Exception ex)
            {
                if (ex is ValidationException vx)
                {
                    return BadRequest(_responseHandler.BadRequest(vx.ErrorCode, vx.Message, vx.Errors));
                }
                else
                {
                    return BadRequest(_responseHandler.BadRequest(CustomErrorCode.IsGetLIst, ex.Message, userId));
                }
            }
        }

        [HttpPost("RequestAcceptOrCancel")]
        public async Task<ActionResult<ResponseModel>> RequestAcceptOrCancelAsync([FromQuery] long requestId, string accpetType)
        {
            try
            {
                List<ValidationError> errors = _validationService.ValidateRequestAccept(requestId,accpetType);
                if (errors.Any())
                {
                    return BadRequest(_responseHandler.BadRequest(CustomErrorCode.IsValid, CustomErrorMessage.ValidationReqType, errors));
                }
                bool isAccept=await _userService.RequestAcceptOrCancelAsync(requestId,accpetType);
                if (!isAccept)
                {
                    return BadRequest(_responseHandler.BadRequest(CustomErrorCode.IsValid, CustomErrorMessage.ValidationReqType, errors));
                }
                return Ok(_responseHandler.Success(CustomErrorMessage.AccpteUpdate, requestId));
            }
            catch (Exception ex)
            {
                if (ex is ValidationException vx)
                {
                    return BadRequest(_responseHandler.BadRequest(vx.ErrorCode, vx.Message, vx.Errors));
                }
                else
                {
                    return BadRequest(_responseHandler.BadRequest(CustomErrorCode.IsGetLIst, ex.Message, requestId));
                }
            }
        }

        [HttpGet("FollowerAndFollowingCountById")]
        public async Task<ActionResult<ResponseModel>> GetFollowerAndFollowingCountByIdAsync([FromQuery] long userId)
        {
            try
            {
                List<ValidationError> errors = _validationService.ValidateGetUserById(userId);
                if (errors.Any())
                {
                    return BadRequest(_responseHandler.BadRequest(CustomErrorCode.IsValid, CustomErrorMessage.ExitsUser, errors));
                }
                CountResponseDTO count = await _userService.GetFollowerAndFollowingCountByIdAsync(userId);
                if (count == null)
                {
                    return BadRequest(_responseHandler.BadRequest(CustomErrorCode.IsCount, CustomErrorMessage.CountError, userId));
                }

                return Ok(_responseHandler.Success(CustomErrorMessage.CountSucces, count));
            }
            catch (Exception ex)
            {
                if (ex is ValidationException vx)
                {
                    return BadRequest(_responseHandler.BadRequest(vx.ErrorCode, vx.Message, vx.Errors));
                }
                else
                {
                    return BadRequest(_responseHandler.BadRequest(CustomErrorCode.IsGetLIst, ex.Message, userId));
                }
            }
        }

        [HttpPost("CreatePostAsync")]
        public async Task<ActionResult<ResponseModel>> CreatePostAsync([FromForm] CreatePostDTO model)
        {

            List<ValidationError> errors = _validationService.ValidateCreatePost(model);
            if (errors.Any())
            {
                return BadRequest(_responseHandler.BadRequest(CustomErrorCode.IsValid, CustomErrorMessage.ValidationPost, errors));
            }
        }
    }
}
