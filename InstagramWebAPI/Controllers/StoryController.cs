﻿using DataAccess.CustomModel;
using InstagramWebAPI.BLL;
using InstagramWebAPI.Common;
using InstagramWebAPI.DTO;
using InstagramWebAPI.Helpers;
using InstagramWebAPI.Interface;
using InstagramWebAPI.Utils;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace InstagramWebAPI.Controllers
{
    [Route("api/Story")]
    [ApiController]
    public class StoryController:ControllerBase
    {
        private readonly IValidationService _validationService;
        private readonly ResponseHandler _responseHandler;
        private readonly IStoryService _storyService;

        public StoryController(IValidationService validationService, ResponseHandler responseHandler, IStoryService storyService)
        {
            _validationService = validationService;
            _responseHandler = responseHandler;
            _storyService = storyService;
        }

        /// <summary>
        /// Adds a new story asynchronously based on the provided data.
        /// </summary>
        /// <param name="model">The data object containing the story file and associated details.</param>
        /// <returns>
        /// An asynchronous task that returns an <see cref="ActionResult{T}"/> where T is <see cref="ResponseModel"/>.
        /// If successful, returns an <see cref="ActionResult"/> with a success message indicating the story was uploaded successfully.
        /// </returns>
        [HttpPost("AddStory")]
        [Authorize]
        public async Task<ActionResult<ResponseModel>> AddStoryAsync([FromForm] AddStoryDTO model)
        {
            try
            {
                List<ValidationError> errors = _validationService.ValidateStoryFile(model);
                if (errors.Any())
                {
                    return BadRequest(_responseHandler.BadRequest(CustomErrorCode.IsValid, CustomErrorMessage.ValidationProfile, errors));
                }

                StoryResponseListDTO storyResponse = await _storyService.AddStoryAsync(model);
                if (storyResponse == null)
                {
                    return BadRequest(_responseHandler.BadRequest(CustomErrorCode.IsStory, CustomErrorMessage.UploadStoryError, model));
                }
                return Ok(_responseHandler.Success(CustomErrorMessage.UploadStory, storyResponse));
            }
            catch (Exception ex)
            {
                if (ex is ValidationException vx)
                {
                    return BadRequest(_responseHandler.BadRequest(vx.ErrorCode, vx.Message, vx.Errors));
                }
                else
                {
                    return BadRequest(_responseHandler.BadRequest(CustomErrorCode.IsStory, ex.Message, model));
                }
            }
        }

        /// <summary>
        /// Deletes a story asynchronously based on the provided story ID.
        /// </summary>
        /// <param name="storyId">The unique identifier of the story to delete.</param>
        /// <returns>
        /// An asynchronous task that returns an <see cref="ActionResult{T}"/> where T is <see cref="ResponseModel"/>.
        /// If successful, returns an <see cref="ActionResult"/> with a success message indicating the story was deleted.
        /// </returns>
        [HttpDelete("DeleteStory")]
        [Authorize]
        public async Task<ActionResult<ResponseModel>> DeteleStoryAsync([FromQuery] long storyId)
        {
            try
            {
                List<ValidationError> errors = _validationService.ValidateStoryId(storyId);
                if (errors.Any())
                {
                    return BadRequest(_responseHandler.BadRequest(CustomErrorCode.IsValid, CustomErrorMessage.ValidationStory, errors));
                }
                bool isDeleted = await _storyService.DeteleStoryAsync(storyId);
                if (!isDeleted)
                {
                    return BadRequest(_responseHandler.BadRequest(CustomErrorCode.IsStoryDelete, CustomErrorMessage.StoryDeleteError, errors));
                }
                return Ok(_responseHandler.Success(CustomErrorMessage.StoryDelete, storyId));
            }
            catch (Exception ex)
            {
                if (ex is ValidationException vx)
                {
                    return BadRequest(_responseHandler.BadRequest(vx.ErrorCode, vx.Message, vx.Errors));
                }
                else
                {
                    return BadRequest(_responseHandler.BadRequest(CustomErrorCode.IsStoryDelete, ex.Message, storyId));
                }
            }
        }

        /// <summary>
        /// Retrieves a list of stories belonging to a user asynchronously based on the provided user ID.
        /// </summary>
        /// <param name="model">The request data object containing the user ID.</param>
        /// <returns>
        /// An asynchronous task that returns an <see cref="ActionResult{T}"/> where T is <see cref="ResponseModel"/>.
        /// If successful, returns an <see cref="ActionResult"/> with a success message and the retrieved story list.
        /// </returns>
        [HttpPost("StoryListByUserId")]
        [Authorize]
        public async Task<ActionResult<ResponseModel>> GetStoryListByUserIdAsync([FromBody] RequestDTO<UserIdRequestDTO> model)
        {
            try
            {
                List<ValidationError> errors = _validationService.ValidateGetUserById(model.Model.UserId);
                if (errors.Any())
                {
                    return BadRequest(_responseHandler.BadRequest(CustomErrorCode.IsValid, CustomErrorMessage.ValidationStory, errors));
                }
                PaginationResponceModel<StoryResponseListDTO> data = await _storyService.GetStoryListByUserIdAsync(model);
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
        /// Marks a story as seen by a user asynchronously based on the provided story ID.
        /// </summary>
        /// <param name="storyId">The unique identifier of the story to mark as seen.</param>
        /// <returns>
        /// An asynchronous task that returns an <see cref="ActionResult{T}"/> where T is <see cref="ResponseModel"/>.
        /// If successful, returns an <see cref="ActionResult"/> with a success message indicating the story was marked as seen.
        /// </returns>
        [HttpPost("StorySeen")]
        [Authorize]
        public async Task<ActionResult<ResponseModel>> StorySeenByUserIdAsync([FromForm] long storyId)
        {
            try
            {
                List<ValidationError> errors = _validationService.ValidateStoryId(storyId);
                if (errors.Any())
                {
                    return BadRequest(_responseHandler.BadRequest(CustomErrorCode.IsValid, CustomErrorMessage.ValidationStory, errors));
                }

                bool isSuccess = await _storyService.StorySeenByUserIdAsync(storyId);
                if (isSuccess == false)
                {
                    return BadRequest(_responseHandler.BadRequest(CustomErrorCode.IsStory, CustomErrorMessage.UploadStoryError, storyId));
                }
                return Ok(_responseHandler.Success(CustomErrorMessage.SeenStory, storyId));
            }
            catch (Exception ex)
            {
                if (ex is ValidationException vx)
                {
                    return BadRequest(_responseHandler.BadRequest(vx.ErrorCode, vx.Message, vx.Errors));
                }
                else
                {
                    return BadRequest(_responseHandler.BadRequest(CustomErrorCode.IsStory, ex.Message, storyId));
                }
            }
        }

        /// <summary>
        /// Retrieves a specific story asynchronously based on the provided user ID and story ID.
        /// </summary>
        /// <param name="userId">The unique identifier of the user who owns the story.</param>
        /// <param name="storyId">The unique identifier of the story to retrieve.</param>
        /// <returns>
        /// An asynchronous task that returns an <see cref="ActionResult{T}"/> where T is <see cref="ResponseModel"/>.
        /// If successful, returns an <see cref="ActionResult"/> with a success message and the retrieved story data.
        /// </returns>
        [HttpGet("GetStoryById")]
        [Authorize]
        public async Task<ActionResult<ResponseModel>> GetStoryByIdAsync([FromQuery] long userId,long storyId)
        {
            try
            {
                List<ValidationError> errors = _validationService.ValidateGetStoryById(userId,storyId);
                if (errors.Any())
                {
                    return BadRequest(_responseHandler.BadRequest(CustomErrorCode.IsValid, CustomErrorMessage.ValidationStory, errors));
                }
                StoryResponseListDTO data = await _storyService.GetStoryById(userId,storyId);
                if (data == null)
                {
                    return BadRequest(_responseHandler.BadRequest(CustomErrorCode.IsNotStory, CustomErrorMessage.ExitsPost, userId));
                }
                return Ok(_responseHandler.Success(CustomErrorMessage.GetStory, data));
            }
            catch (Exception ex)
            {
                if (ex is ValidationException vx)
                {
                    return BadRequest(_responseHandler.BadRequest(vx.ErrorCode, vx.Message, vx.Errors));
                }
                else
                {
                    return BadRequest(_responseHandler.BadRequest(CustomErrorCode.IsNotStory, ex.Message, storyId));
                }
            }
        }

        /// <summary>
        /// Likes or unlikes a story asynchronously based on the provided story ID and like status.
        /// </summary>
        /// <param name="storyId">The unique identifier of the story to like or unlike.</param>
        /// <param name="isLike">Boolean indicating whether to like (true) or unlike (false) the story.</param>
        /// <returns>
        /// An asynchronous task that returns an <see cref="ActionResult{T}"/> where T is <see cref="ResponseModel"/>.
        /// If successful, returns an <see cref="ActionResult"/> with a success message indicating the story was liked or unliked.
        /// </returns>
        [HttpPost("LikeStory")]
        [Authorize]
        public async Task<ActionResult<ResponseModel>> LikeStoryAsync([FromForm] long storyId,bool isLike)
        {
            try
            {
                List<ValidationError> errors = _validationService.ValidateStoryId(storyId);
                if (errors.Any())
                {
                    return BadRequest(_responseHandler.BadRequest(CustomErrorCode.IsValid, CustomErrorMessage.ValidationStory, errors));
                }

                bool isSuccess = await _storyService.LikeStoryAsync(storyId,isLike);
                if (isSuccess == false)
                {
                    return BadRequest(_responseHandler.BadRequest(CustomErrorCode.IsLIkeStory, CustomErrorMessage.LikeStoryError, storyId));
                }
                return Ok(_responseHandler.Success(CustomErrorMessage.LIkeStory, storyId));
            }
            catch (Exception ex)
            {
                if (ex is ValidationException vx)
                {
                    return BadRequest(_responseHandler.BadRequest(vx.ErrorCode, vx.Message, vx.Errors));
                }
                else
                {
                    return BadRequest(_responseHandler.BadRequest(CustomErrorCode.IsLIkeStory, ex.Message, storyId));
                }
            }
        }

    }
}
