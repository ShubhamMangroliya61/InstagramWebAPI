using DataAccess.CustomModel;
using InstagramWebAPI.BLL;
using InstagramWebAPI.Common;
using InstagramWebAPI.DAL.Models;
using InstagramWebAPI.DTO;
using InstagramWebAPI.Helpers;
using InstagramWebAPI.Interface;
using InstagramWebAPI.Utils;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.JsonPatch.Internal;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using System.Transactions;
using System.Xml.Linq;

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

        public UserController(IValidationService validationService, ResponseHandler responseHandler, IUserService userService, IAuthService authService, Helper helper)
        {
            _validationService = validationService;
            _responseHandler = responseHandler;
            _userService = userService;
            _authService = authService;
            _helper = helper;
        }
        /// <summary>
        /// Handles the asynchronous upload of a user's profile photo.
        /// </summary>
        /// <param name="model">The data transfer object containing the profile photo to upload.</param>
        /// <returns>An <see cref="ActionResult{T}"/> representing the result of the profile photo upload operation.</returns>
        [HttpPost("UploadProfilePhoto")]
        [Authorize]
        public async Task<ActionResult> UploadProfilePhotoAsync(IFormFile? ProfilePhoto)
        {
            try
            {
                long userId = _helper.GetUserIdClaim();
                List<ValidationError> errors = _validationService.ValidateProfileFile(ProfilePhoto,userId);
                if (errors.Any())
                {
                    return BadRequest(_responseHandler.BadRequest(CustomErrorCode.IsValid, CustomErrorMessage.ValidationProfile, errors));
                }

                ProfilePhotoResponseDTO profilePhotoDTO = await _userService.UploadProfilePhotoAsync(ProfilePhoto, userId);
                if (profilePhotoDTO == null)
                {
                    return BadRequest(_responseHandler.BadRequest(CustomErrorCode.IsUpload, CustomErrorMessage.UploadError, ""));
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
                    return BadRequest(_responseHandler.BadRequest(CustomErrorCode.IsUpload, ex.Message, ""));
                }
            }
        }
        
        /// <summary>
        /// Handles the asynchronous update of user profile information.
        /// </summary>
        /// <param name="model">The data transfer object containing the updated user profile data.</param>
        /// <returns>An <see cref="ActionResult{T}"/> representing the result of the profile update operation.</returns>
        [Authorize]
        [HttpPost("UpdateProfile")]
        public async Task<ActionResult> UpdateProfileAsync(UserDTO model)
        {
            try
            {
                model.UserId = _helper.GetUserIdClaim();
                List<ValidationError> errors = _validationService.ValidateRegistration(model);
                if (errors.Any())
                {
                    return BadRequest(_responseHandler.BadRequest(CustomErrorCode.IsValid, CustomErrorMessage.ValidationUpdateProfile, errors));
                }

                UserDTO? user = await _authService.UpSertUserAsync(model);
                if (user == null)
                {
                    return BadRequest(_responseHandler.BadRequest(CustomErrorCode.IsUpdate, CustomErrorMessage.UpdateProfile, ""));
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
                    return BadRequest(_responseHandler.BadRequest(CustomErrorCode.IsUpload, ex.Message, ""));
                }
            }
        }

        /// <summary>
        /// Handles the asynchronous processing of a follow request.
        /// </summary>
        /// <param name="model">The data transfer object containing follow request details.</param>
        /// <returns>An <see cref="ActionResult{T}"/> representing the result of the follow request operation.</returns>
        [HttpPost("FollowRequest")]
        [Authorize]
        public async Task<ActionResult> FollowRequestAsync(FollowRequestDTO model)
        {
            try
            {
                long fromUserId = _helper.GetUserIdClaim();
                List<ValidationError> errors = _validationService.ValidateFollowRequest(model,fromUserId);
                if (errors.Any())
                {
                    return BadRequest(_responseHandler.BadRequest(CustomErrorCode.IsValid, CustomErrorMessage.ValidationRequest, errors));
                }
                bool isFollow = await _userService.FollowRequestAsync(model,fromUserId);
                if (!isFollow)
                {
                    return BadRequest(_responseHandler.BadRequest(CustomErrorCode.IsRequest, CustomErrorMessage.RequestError, ""));
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
                    return BadRequest(_responseHandler.BadRequest(CustomErrorCode.IsUpload, ex.Message, ""));
                }
            }
        }

        /// <summary>
        /// Retrieves a list of followers or following users based on the provided request asynchronously.
        /// </summary>
        /// <param name="model">The model containing request details for fetching follower or following users.</param>
        /// <returns>An <see cref="ActionResult{T}"/> representing the result of the operation, containing a pagination response of user data.</returns>
        [HttpPost("FollowerORFollowingListById")]
        [Authorize]
        public async Task<ActionResult> GetFollowerORFollowingListByIdAsync([FromBody] RequestDTO<FollowerListRequestDTO> model)
        {
            try
            {
                List<ValidationError> errors = _validationService.ValidateFollowerList(model);
                if (errors.Any())
                {
                    return BadRequest(_responseHandler.BadRequest(CustomErrorCode.IsValid, CustomErrorMessage.ValidationRequest, errors));
                }
                PaginationResponceModel<UserDTO> data = await _userService.GetFollowerORFollowingListAsync(model);
                if (data == null)
                {
                    return BadRequest(_responseHandler.BadRequest(CustomErrorCode.IsGetLIst, CustomErrorMessage.GetFollowerList, ""));
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
                    return BadRequest(_responseHandler.BadRequest(CustomErrorCode.IsGetLIst, ex.Message, ""));
                }
            }
        }

        /// <summary>
        /// Retrieves a paginated list of requests based on the provided request data asynchronously.
        /// </summary>
        /// <param name="model">The model containing request details for fetching the list of requests.</param>
        /// <returns>An <see cref="ActionResult{T}"/> representing the result of the request list retrieval operation.</returns>
        [HttpPost("RequestListById")]
        [Authorize]
        public async Task<ActionResult> GetRequestListByIdAsync([FromBody] RequestDTO<FollowRequestDTO> model)
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
                    return BadRequest(_responseHandler.BadRequest(CustomErrorCode.IsGetLIst, CustomErrorMessage.GetFollowerList, ""));
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
                    return BadRequest(_responseHandler.BadRequest(CustomErrorCode.IsGetLIst, ex.Message, ""));
                }
            }
        }

        /// <summary>
        /// Retrieves a paginated list of users based on the provided username search criteria.
        /// </summary>
        /// <param name="model">Request DTO containing the user ID and pagination parameters.</param>
        /// <returns>
        /// An ActionResult representing the result of the operation.
        /// If successful, returns HTTP 200 (OK) with a pagination response model containing the list of users.
        /// If validation fails or an error occurs, returns HTTP 400 (Bad Request) with an error message.
        /// </returns>
        [HttpPost("GetUserListByUserName")]
        [Authorize]
        public async Task<ActionResult> GetUserListByUserNameAsync([FromBody] RequestDTO<UserIdRequestDTO> model)
        {
            try
            {
                List<ValidationError> errors = _validationService.ValidateGetUserById(model.Model.UserId);
                if (errors.Any())
                {
                    return BadRequest(_responseHandler.BadRequest(CustomErrorCode.IsValid, CustomErrorMessage.ValidationList, errors));
                }
                PaginationResponceModel<UserDTO> data = await _userService.GetUserListByUserNameAsync(model);
                if (data == null)
                {
                    return BadRequest(_responseHandler.BadRequest(CustomErrorCode.IsGetLIst, CustomErrorMessage.GetFollowerList, ""));
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
                    return BadRequest(_responseHandler.BadRequest(CustomErrorCode.IsGetLIst, ex.Message, ""));
                }
            }
        }

        /// <summary>
        /// Retrieves user data by user ID asynchronously.
        /// </summary>
        /// <param name="userId">The ID of the user to retrieve.</param>
        /// <returns>An <see cref="ActionResult{T}"/> representing the result of the user retrieval operation.</returns>
        [HttpGet("GetUserById")]
        [Authorize]
        public async Task<ActionResult> GetUserById(long userId)
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
                    return BadRequest(_responseHandler.BadRequest(CustomErrorCode.IsNotExits, CustomErrorMessage.ExitsUser, ""));
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
                    return BadRequest(_responseHandler.BadRequest(CustomErrorCode.IsGetLIst, ex.Message, ""));
                }
            }
        }

        /// <summary>
        /// Handles accepting or canceling a request asynchronously.
        /// </summary>
        /// <param name="requestId">The ID of the request to accept or cancel.</param>
        /// <param name="acceptType">The type of action to perform (accept or cancel).</param>
        /// <returns>An <see cref="ActionResult{T}"/> representing the result of the request accept or cancel operation.</returns>
        [HttpPost("RequestAcceptOrCancel")]
        [Authorize]
        public async Task<ActionResult> RequestAcceptOrCancelAsync([FromQuery] long requestId, string accpetType)
        {
            try
            {
                List<ValidationError> errors = _validationService.ValidateRequestAccept(requestId, accpetType);
                if (errors.Any())
                {
                    return BadRequest(_responseHandler.BadRequest(CustomErrorCode.IsValid, CustomErrorMessage.ValidationReqType, errors));
                }
                bool isAccept = await _userService.RequestAcceptOrCancelAsync(requestId, accpetType);
                if (!isAccept)
                {
                    return BadRequest(_responseHandler.BadRequest(CustomErrorCode.IsValid, CustomErrorMessage.ValidationReqType, ""));
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
                    return BadRequest(_responseHandler.BadRequest(CustomErrorCode.IsGetLIst, ex.Message, ""));
                }
            }
        }

        /// <summary>
        /// Retrieves the follower and following count for a user asynchronously.
        /// </summary>
        /// <param name="userId">The ID of the user to retrieve follower and following counts.</param>
        /// <returns>An <see cref="ActionResult{T}"/> representing the result of retrieving follower and following counts.</returns>
        [HttpGet("FollowerAndFollowingAndPostCountById/{userId}")]
        [Authorize]
        public async Task<ActionResult> GetFollowerAndFollowingAndPostCountByIdAsync(long userId)
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
                    return BadRequest(_responseHandler.BadRequest(CustomErrorCode.IsCount, CustomErrorMessage.CountError, ""));
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
                    return BadRequest(_responseHandler.BadRequest(CustomErrorCode.IsGetLIst, ex.Message, ""));
                }
            }
        }

        /// <summary>
        /// Retrieves mutual friends between two users asynchronously based on the provided user ID.
        /// </summary>
        /// <param name="model">The request data object containing the user ID.</param>
        /// <returns>
        /// An asynchronous task that returns an <see cref="ActionResult{T}"/> where T is <see cref="ResponseModel"/>.
        /// If successful, returns an <see cref="ActionResult"/> with a success message and the list of mutual friends with details.
        /// </returns>
        [HttpPost("MutualFriendAsync")]
        [Authorize]
        public async Task<ActionResult> MutualFriendAsync([FromBody] RequestDTO<UserIdRequestDTO> model)
        {
            try
            {
                List<ValidationError> errors = _validationService.ValidateMatualFrnd(model);
                if (errors.Any())
                {
                    return BadRequest(_responseHandler.BadRequest(CustomErrorCode.IsValid, CustomErrorMessage.ExitsUser, errors));
                }
                PaginationResponceModel<MutualFriendDTO> data = await _userService.GetMutualFriendsWithDetailsAsync(model);
                if (data == null)
                {
                    return BadRequest(_responseHandler.BadRequest(CustomErrorCode.IsMutual, CustomErrorMessage.MutualError, ""));
                }

                return Ok(_responseHandler.Success(CustomErrorMessage.mutualSucces, data));
            }
            catch (Exception ex)
            {
                if (ex is ValidationException vx)
                {
                    return BadRequest(_responseHandler.BadRequest(vx.ErrorCode, vx.Message, vx.Errors));
                }
                else
                {
                    return BadRequest(_responseHandler.BadRequest(CustomErrorCode.IsMutual, ex.Message, ""));
                }
            }
        }

        /// <summary>
        /// Retrieves a paginated list of suggested users based on the logged-in user's profile.
        /// </summary>
        /// <param name="model">Pagination parameters including page number and page size.</param>
        /// <returns>
        /// An ActionResult representing the result of the operation.
        /// If successful, returns HTTP 200 (OK) with a pagination response containing the list of suggested users.
        /// If there are no suggestions or an error occurs, returns HTTP 400 (Bad Request) with an error message.
        /// </returns>
        [HttpPost("GetSuggestionList")]
        [Authorize]
        public async Task<ActionResult> GetSuggestionListAsync([FromBody] PaginationRequestDTO model)
        {
            try
            {
                PaginationResponceModel<UserDTO> data = await _userService.GetSuggestionListAsync(model);
                if (data == null)
                {
                    return BadRequest(_responseHandler.BadRequest(CustomErrorCode.IsSuggestion, CustomErrorMessage.SuggestionError, ""));
                }

                return Ok(_responseHandler.Success(CustomErrorMessage.suggestionSucces, data));
            }
            catch (Exception ex)
            {
                if (ex is ValidationException vx)
                {
                    return BadRequest(_responseHandler.BadRequest(vx.ErrorCode, vx.Message, vx.Errors));
                }
                else
                {
                    return BadRequest(_responseHandler.BadRequest(CustomErrorCode.IsSuggestion, ex.Message, ""));
                }
            }
        }

        /// <summary>
        /// Upserts a search user by their ID, indicating the logged-in user's interest in following or unfollowing them.
        /// </summary>
        /// <param name="searchUserId">The ID of the user to be followed or unfollowed.</param>
        /// <returns>
        /// An ActionResult representing the result of the operation.
        /// If successful, returns HTTP 200 (OK) with a success message.
        /// If the user ID is invalid or an error occurs, returns HTTP 400 (Bad Request) with an error message.
        /// </returns>
        [HttpPost("UpsertSearchUserById")]
        [Authorize]
        public async Task<ActionResult> UpsertSearchUserByIdAsync(long searchUserId)
        {
            try
            {
                List<ValidationError> errors = _validationService.ValidateGetUserById(searchUserId);
                if (errors.Any())
                {
                    return BadRequest(_responseHandler.BadRequest(CustomErrorCode.IsValid, CustomErrorMessage.ExitsUser, errors));
                }
                bool isFollow = await _userService.UpsertSearchUserById(searchUserId);
                if (!isFollow)
                {
                    return BadRequest(_responseHandler.BadRequest(CustomErrorCode.IsUser, CustomErrorMessage.SearchUserError, ""));
                }
                return Ok(_responseHandler.Success(CustomErrorMessage.SearchUSerSuccess, searchUserId));
            }
            catch (Exception ex)
            {
                if (ex is ValidationException vx)
                {
                    return BadRequest(_responseHandler.BadRequest(vx.ErrorCode, vx.Message, vx.Errors));
                }
                else
                {
                    return BadRequest(_responseHandler.BadRequest(CustomErrorCode.IsUser, ex.Message, ""));
                }
            }
        }

        /// <summary>
        /// Retrieves a paginated list of search records based on the provided pagination parameters.
        /// </summary>
        /// <param name="model">Pagination parameters including page number and page size.</param>
        /// <returns>
        /// An ActionResult representing the result of the operation.
        /// If successful, returns HTTP 200 (OK) with a pagination response containing the list of search records.
        /// If the request fails validation or an error occurs, returns HTTP 400 (Bad Request) with an error message.
        /// </returns>
        [HttpPost("GetSearchUserList")]
        [Authorize]
        public async Task<ActionResult> GetSearchUserListAsync([FromBody] PaginationRequestDTO model)
        {
            try
            {
                PaginationResponceModel<SearchDTO> data = await _userService.GetSearchUserList(model);
                if (data == null)
                {
                    return BadRequest(_responseHandler.BadRequest(CustomErrorCode.IsGetLIst, CustomErrorMessage.GetFollowerList, ""));
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
                    return BadRequest(_responseHandler.BadRequest(CustomErrorCode.IsGetLIst, ex.Message, ""));
                }
            }
        }

        /// <summary>
        /// Deletes a search record identified by the provided search ID.
        /// </summary>
        /// <param name="searchId">The ID of the search record to delete.</param>
        /// <returns>
        /// An ActionResult representing the result of the operation.
        /// If successful, returns HTTP 200 (OK) with a success message indicating the deletion of the search record.
        /// If the request fails validation or an error occurs, returns HTTP 400 (Bad Request) with an error message.
        /// </returns>
        [HttpPost("DeteleSearchUser")]
        [Authorize]
        public async Task<ActionResult> DeteleSearchUserAsync([FromQuery] long searchId)
        {
            try
            {
                List<ValidationError> errors = _validationService.ValidateSerachId(searchId);
                if (errors.Any())
                {
                    return BadRequest(_responseHandler.BadRequest(CustomErrorCode.IsValid, CustomErrorMessage.ValidationSearchUser, errors)); ;
                }
                bool isDeleted = await _userService.DeleteSearchUser(searchId);
                if (!isDeleted)
                {
                    return BadRequest(_responseHandler.BadRequest(CustomErrorCode.IsUser, CustomErrorMessage.SearchUserError1, ""));
                }
                return Ok(_responseHandler.Success(CustomErrorMessage.SearchUserDelete, searchId));
            }
            catch (Exception ex)
            {
                if (ex is ValidationException vx)
                {
                    return BadRequest(_responseHandler.BadRequest(vx.ErrorCode, vx.Message, vx.Errors));
                }
                else
                {
                    return BadRequest(_responseHandler.BadRequest(CustomErrorCode.IsUser, ex.Message, ""));
                }
            }
        }
    }
}
