﻿using DataAccess.CustomModel;
using InstagramWebAPI.Common;
using InstagramWebAPI.DAL.Models;
using InstagramWebAPI.DTO;
using InstagramWebAPI.Helpers;
using InstagramWebAPI.Interface;
using InstagramWebAPI.Utils;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.JsonPatch.Internal;
using Microsoft.AspNetCore.Mvc;
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

        public UserController(IValidationService validationService, ResponseHandler responseHandler, IUserService userService, IAuthService authService)
        {
            _validationService = validationService;
            _responseHandler = responseHandler;
            _userService = userService;
            _authService = authService;
            this._helper = new Helper();
        }
        /// <summary>
        /// Handles the asynchronous upload of a user's profile photo.
        /// </summary>
        /// <param name="model">The data transfer object containing the profile photo to upload.</param>
        /// <returns>An <see cref="ActionResult{T}"/> representing the result of the profile photo upload operation.</returns>
        [HttpPost("UploadProfilePhoto")]
        [Authorize]
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

        /// <summary>
        /// Handles the asynchronous update of user profile information.
        /// </summary>
        /// <param name="model">The data transfer object containing the updated user profile data.</param>
        /// <returns>An <see cref="ActionResult{T}"/> representing the result of the profile update operation.</returns>
        [Authorize]
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

        /// <summary>
        /// Handles the asynchronous processing of a follow request.
        /// </summary>
        /// <param name="model">The data transfer object containing follow request details.</param>
        /// <returns>An <see cref="ActionResult{T}"/> representing the result of the follow request operation.</returns>
        [HttpPost("FollowRequest")]
        [Authorize]
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

        /// <summary>
        /// Retrieves a list of followers or following users based on the provided request asynchronously.
        /// </summary>
        /// <param name="model">The model containing request details for fetching follower or following users.</param>
        /// <returns>An <see cref="ActionResult{T}"/> representing the result of the operation, containing a pagination response of user data.</returns>
        [HttpPost("FollowerORFollowingListById")]
        [Authorize]
        public async Task<ActionResult<ResponseModel>> GetFollowerORFollowingListByIdAsync([FromBody] RequestDTO<FollowerListRequestDTO> model)
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

        /// <summary>
        /// Retrieves a paginated list of requests based on the provided request data asynchronously.
        /// </summary>
        /// <param name="model">The model containing request details for fetching the list of requests.</param>
        /// <returns>An <see cref="ActionResult{T}"/> representing the result of the request list retrieval operation.</returns>
        [HttpPost("RequestListById")]
        [Authorize]
        public async Task<ActionResult<ResponseModel>> GetRequestListByIdAsync([FromBody] RequestDTO<FollowRequestDTO> model)
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

        /// <summary>
        /// Retrieves user data by user ID asynchronously.
        /// </summary>
        /// <param name="userId">The ID of the user to retrieve.</param>
        /// <returns>An <see cref="ActionResult{T}"/> representing the result of the user retrieval operation.</returns>
        [HttpGet("GetUserById")]
        [Authorize]
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

        /// <summary>
        /// Handles accepting or canceling a request asynchronously.
        /// </summary>
        /// <param name="requestId">The ID of the request to accept or cancel.</param>
        /// <param name="acceptType">The type of action to perform (accept or cancel).</param>
        /// <returns>An <see cref="ActionResult{T}"/> representing the result of the request accept or cancel operation.</returns>
        [HttpPost("RequestAcceptOrCancel")]
        [Authorize]
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

        /// <summary>
        /// Retrieves the follower and following count for a user asynchronously.
        /// </summary>
        /// <param name="userId">The ID of the user to retrieve follower and following counts.</param>
        /// <returns>An <see cref="ActionResult{T}"/> representing the result of retrieving follower and following counts.</returns>
        [HttpGet("FollowerAndFollowingCountById")]
        [Authorize]
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

        /// <summary>
        /// Creates a new post asynchronously.
        /// </summary>
        /// <param name="model">The data transfer object containing post creation details.</param>
        /// <returns>An <see cref="ActionResult{T}"/> representing the result of the post creation operation.</returns>
        [HttpPost("CreatePostAsync")]
        [Authorize]
        public async Task<ActionResult<ResponseModel>> CreatePostAsync([FromForm] CreatePostDTO model)
        {
            try
            {
                //using var transaction = new TransactionScope();
                List<ValidationError> errors = _validationService.ValidateCreatePost(model);
                if (errors.Any())
                {
                    return BadRequest(_responseHandler.BadRequest(CustomErrorCode.IsValid, CustomErrorMessage.ValidationPost, errors));
                }
                PostResponseDTO responseDTO = await _userService.CreatePostAsync(model);
                if (responseDTO == null)
                {
                    return BadRequest(_responseHandler.BadRequest(CustomErrorCode.IsPOst, CustomErrorMessage.PostError, model));
                }
                if (model.PostId <= 0)
                {
                    return Ok(_responseHandler.Success(CustomErrorMessage.CreatePost, responseDTO));
                }
                else
                {
                    return Ok(_responseHandler.Success(CustomErrorMessage.UpdatePost, responseDTO));
                }
            }
            catch (Exception ex)
            {
                if (ex is ValidationException vx)
                {
                    return BadRequest(_responseHandler.BadRequest(vx.ErrorCode, vx.Message, vx.Errors));
                }
                else
                {
                    return BadRequest(_responseHandler.BadRequest(CustomErrorCode.IsPOst, ex.Message, model));
                }
            }
        }

        [HttpPost("PostAndReelById")]
        public async Task<ActionResult<ResponseModel>> GetPostAndReelByIdAsync([FromBody] RequestDTO<PostListRequestDTO> model)
        {
            try
            {
                List<ValidationError> errors = _validationService.ValidatePostList(model);
                if (errors.Any())
                {
                    return BadRequest(_responseHandler.BadRequest(CustomErrorCode.IsValid, CustomErrorMessage.ValidationPost, errors));
                }
                PaginationResponceModel<PostResponseDTO> data = await _userService.GetPostsByIdAsync(model);
                if (data == null)
                {
                    return BadRequest(_responseHandler.BadRequest(CustomErrorCode.IsGetLIst, CustomErrorMessage.GetFollowerList, model));
                }
                return Ok(_responseHandler.Success(CustomErrorMessage.GetPostListSucces, data));
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

        [HttpPost("DeletePost")]
        public async Task<ActionResult<ResponseModel>> DetelePostAsync([FromQuery] long postId)
        {
            try
            {
                List<ValidationError> errors = _validationService.ValidateDeletePostId(postId);
                if (errors.Any())
                {
                    return BadRequest(_responseHandler.BadRequest(CustomErrorCode.IsValid, CustomErrorMessage.ValidationPost, errors));
                }
                bool isDeleted = await _userService.DetelePostAsync(postId);
                if (!isDeleted)
                {
                    return BadRequest(_responseHandler.BadRequest(CustomErrorCode.IsPostDelete, CustomErrorMessage.PostDeleteError, errors));
                }
                return Ok(_responseHandler.Success(CustomErrorMessage.PostDelete, postId));
            }
            catch (Exception ex)
            {
                if (ex is ValidationException vx)
                {
                    return BadRequest(_responseHandler.BadRequest(vx.ErrorCode, vx.Message, vx.Errors));
                }
                else
                {
                    return BadRequest(_responseHandler.BadRequest(CustomErrorCode.IsGetLIst, ex.Message, postId));
                }
            }
        }

        public async Task<ActionResult<ResponseModel>> LikePostAsync(long userId,long postId)
        {
            try
            {
                List<ValidationError> errors = _validationService.ValidateLikePost(userId,postId);
                if (errors.Any())
                {
                    return BadRequest(_responseHandler.BadRequest(CustomErrorCode.IsValid, CustomErrorMessage.ValidationPost, errors));
                }
                bool isDeleted = await _userService.DetelePostAsync(postId);
                if (!isDeleted)
                {
                    return BadRequest(_responseHandler.BadRequest(CustomErrorCode.IsPostDelete, CustomErrorMessage.PostDeleteError, errors));
                }
                return Ok(_responseHandler.Success(CustomErrorMessage.PostDelete, postId));
            }
            catch (Exception ex)
            {
                if (ex is ValidationException vx)
                {
                    return BadRequest(_responseHandler.BadRequest(vx.ErrorCode, vx.Message, vx.Errors));
                }
                else
                {
                    return BadRequest(_responseHandler.BadRequest(CustomErrorCode.IsGetLIst, ex.Message, postId));
                }
            }
        }
    }
}
