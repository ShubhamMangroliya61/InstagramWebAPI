using DataAccess.CustomModel;
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
        private readonly Helper _helper;
        public StoryController(IValidationService validationService, ResponseHandler responseHandler, IStoryService storyService, Helper helper)
        {
            _validationService = validationService;
            _responseHandler = responseHandler;
            _storyService = storyService;
            _helper = helper;
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
        public async Task<ActionResult> AddStoryAsync([FromForm] AddStoryDTO model)
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
                    return BadRequest(_responseHandler.BadRequest(CustomErrorCode.IsStory, CustomErrorMessage.UploadStoryError, ""));
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
                    return BadRequest(_responseHandler.BadRequest(CustomErrorCode.IsStory, ex.Message, ""));
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
        [HttpPost("DeleteStory")]
        [Authorize]
        public async Task<ActionResult> DeteleStoryAsync(long storyId)
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
                    return BadRequest(_responseHandler.BadRequest(CustomErrorCode.IsStoryDelete, CustomErrorMessage.StoryDeleteError, ""));
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
                    return BadRequest(_responseHandler.BadRequest(CustomErrorCode.IsStoryDelete, ex.Message, ""));
                }
            }
        }

        /// <summary>
        /// Retrieves a paginated list of stories for the logged-in user.
        /// </summary>
        /// <param name="model">Pagination parameters including page number and page size.</param>
        /// <returns>
        /// An ActionResult representing the result of the operation.
        /// If successful, returns HTTP 200 (OK) with a pagination response containing the story list.
        /// If validation fails or an error occurs, returns HTTP 400 (Bad Request) with an error message.
        /// </returns>
        [HttpPost("GetStoryListById")]
        //[Authorize]
        public async Task<ActionResult> GetStoryListByIdAsync([FromBody] PaginationRequestDTO model)
        {
            try
            {
                PaginationResponceModel<StoryResponseListDTO> data = await _storyService.GetStoryListByIdAsync(model);
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
        /// Retrieves a list of stories belonging to a user asynchronously based on the provided user ID.
        /// </summary>
        /// <param name="model">The request data object containing the user ID.</param>
        /// <returns>
        /// An asynchronous task that returns an <see cref="ActionResult{T}"/> where T is <see cref="ResponseModel"/>.
        /// If successful, returns an <see cref="ActionResult"/> with a success message and the retrieved story list.
        /// </returns>
        [HttpPost("StoryListByUserId")]
        [Authorize]
        public async Task<ActionResult> GetStoryListByUserIdAsync([FromBody] RequestDTO<UserIdRequestDTO> model)
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
        /// Marks a story as seen by a user asynchronously based on the provided story ID.
        /// </summary>
        /// <param name="storyId">The unique identifier of the story to mark as seen.</param>
        /// <returns>
        /// An asynchronous task that returns an <see cref="ActionResult{T}"/> where T is <see cref="ResponseModel"/>.
        /// If successful, returns an <see cref="ActionResult"/> with a success message indicating the story was marked as seen.
        /// </returns>
        [HttpPost("StorySeen")]
        [Authorize]
        public async Task<ActionResult> StorySeenByUserIdAsync([FromForm] long storyId)
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
                    return BadRequest(_responseHandler.BadRequest(CustomErrorCode.IsStory, CustomErrorMessage.UploadStoryError, ""));
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
                    return BadRequest(_responseHandler.BadRequest(CustomErrorCode.IsStory, ex.Message, ""));
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
        public async Task<ActionResult> GetStoryByIdAsync([FromQuery] long userId,long storyId)
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
                    return BadRequest(_responseHandler.BadRequest(CustomErrorCode.IsNotStory, CustomErrorMessage.ExitsPost, ""));
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
                    return BadRequest(_responseHandler.BadRequest(CustomErrorCode.IsNotStory, ex.Message, ""));
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
        public async Task<ActionResult> LikeStoryAsync([FromForm] long storyId,bool isLike)
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
                    return BadRequest(_responseHandler.BadRequest(CustomErrorCode.IsLIkeStory, CustomErrorMessage.LikeStoryError, ""));
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
                    return BadRequest(_responseHandler.BadRequest(CustomErrorCode.IsLIkeStory, ex.Message, ""));
                }
            }
        }

        /// <summary>
        /// Creates or updates a highlight for the logged-in user.
        /// </summary>
        /// <param name="model">Form data containing the highlight details.</param>
        /// <returns>
        /// An ActionResult representing the result of the operation.
        /// If successful, returns HTTP 200 (OK) with the created or updated highlight details.
        /// If validation fails or an error occurs, returns HTTP 400 (Bad Request) with an error message.
        /// </returns>
        [HttpPost("UpsertHighlight")]
        [Authorize]
        public async Task<ActionResult> UpsertHighlightAsync([FromForm] HighLightRequestDTO model)
        {
            try
            {
                List<ValidationError> errors = _validationService.ValidateUpsertHighLight(model);
                if (errors.Any())
                {
                    return BadRequest(_responseHandler.BadRequest(CustomErrorCode.IsValid, CustomErrorMessage.ValidationHighLight, errors));
                }

                HighlightDTO response = await _storyService.UpsertHighlightAsync(model);
                if (response == null)
                {
                    return BadRequest(_responseHandler.BadRequest(CustomErrorCode.IsHighlight, CustomErrorMessage.HighlightError, ""));
                }
                return Ok(_responseHandler.Success(CustomErrorMessage.AddHighLight, response));
            }
            catch (Exception ex)
            {
                if (ex is ValidationException vx)
                {
                    return BadRequest(_responseHandler.BadRequest(vx.ErrorCode, vx.Message, vx.Errors));
                }
                else
                {
                    return BadRequest(_responseHandler.BadRequest(CustomErrorCode.IsHighlight, ex.Message, ""));
                }
            }
        }

        /// <summary>
        /// Deletes a highlight identified by its ID.
        /// </summary>
        /// <param name="highLightId">The ID of the highlight to delete.</param>
        /// <returns>
        /// An ActionResult representing the result of the operation.
        /// If successful, returns HTTP 200 (OK) with the ID of the deleted highlight.
        /// If validation fails or an error occurs, returns HTTP 400 (Bad Request) with an error message.
        /// </returns>
        [HttpPost("DeteleHighLight")]
        [Authorize]
        public async Task<ActionResult> DeteleHighLightAsync([FromQuery] long highLightId)
        {
            try
            {
                List<ValidationError> errors = _validationService.ValidateHighLightId(highLightId);
                if (errors.Any())
                {
                    return BadRequest(_responseHandler.BadRequest(CustomErrorCode.IsValid, CustomErrorMessage.ValidationHighLight, errors));
                }
                bool isDeleted = await _storyService.DeleteStoryHighLightAsync(highLightId);
                if (!isDeleted)
                {
                    return BadRequest(_responseHandler.BadRequest(CustomErrorCode.IsHighlight, CustomErrorMessage.HighlightDeleteError, ""));
                }
                return Ok(_responseHandler.Success(CustomErrorMessage.HighlightDelete, highLightId));
            }
            catch (Exception ex)
            {
                if (ex is ValidationException vx)
                {
                    return BadRequest(_responseHandler.BadRequest(vx.ErrorCode, vx.Message, vx.Errors));
                }
                else
                {
                    return BadRequest(_responseHandler.BadRequest(CustomErrorCode.IsHighlight, ex.Message, ""));
                }
            }
        }

        /// <summary>
        /// Adds a story to a highlight identified by their IDs.
        /// </summary>
        /// <param name="highLightId">The ID of the highlight to which the story will be added.</param>
        /// <param name="storyId">The ID of the story to add to the highlight.</param>
        /// <returns>
        /// An ActionResult representing the result of the operation.
        /// If successful, returns HTTP 200 (OK) with the ID of the updated highlight.
        /// If validation fails or an error occurs, returns HTTP 400 (Bad Request) with an error message.
        /// </returns>
        [HttpPost("AddStoryHighLight")]
        [Authorize]
        public async Task<ActionResult> AddStoryHighLightAsync([FromQuery] long highLightId,long storyId)
        {
            try
            {
                long userId=_helper.GetUserIdClaim();
                List<ValidationError> errors = _validationService.ValidateAddStoryhighlight(highLightId,storyId,userId);
                if (errors.Any())
                {
                    return BadRequest(_responseHandler.BadRequest(CustomErrorCode.IsValid, CustomErrorMessage.ValidationHighLight, errors));
                }
                bool isDeleted = await _storyService.AddStoryHighLightAsync(highLightId,storyId);
                if (!isDeleted)
                {
                    return BadRequest(_responseHandler.BadRequest(CustomErrorCode.IsHighlight, CustomErrorMessage.StoryHighlightAddError, ""));
                }
                return Ok(_responseHandler.Success(CustomErrorMessage.StoryHighlightAdd, highLightId));
            }
            catch (Exception ex)
            {
                if (ex is ValidationException vx)
                {
                    return BadRequest(_responseHandler.BadRequest(vx.ErrorCode, vx.Message, vx.Errors));
                }
                else
                {
                    return BadRequest(_responseHandler.BadRequest(CustomErrorCode.IsHighlight, ex.Message, ""));
                }
            }
        }

        /// <summary>
        /// Deletes a story highlight by its ID.
        /// </summary>
        /// <param name="storyHighLightId">The ID of the story highlight to delete.</param>
        /// <returns>
        /// An ActionResult representing the result of the operation.
        /// If successful, returns HTTP 200 (OK) with the ID of the deleted story highlight.
        /// If validation fails or an error occurs, returns HTTP 400 (Bad Request) with an error message.
        /// </returns>
        [HttpPost("DeleteStoryHighLight")]
        [Authorize]
        public async Task<ActionResult> DeleteStoryHighLightAsync([FromQuery] long storyHighLightId)
        {
            try
            {
                List<ValidationError> errors = _validationService.ValidateStoryHighLightId(storyHighLightId);
                if (errors.Any())
                {
                    return BadRequest(_responseHandler.BadRequest(CustomErrorCode.IsValid, CustomErrorMessage.ValidationHighLight, errors));
                }
                bool isDeleted = await _storyService.DeleteStoryHighLightAsync(storyHighLightId);
                if (!isDeleted)
                {
                    return BadRequest(_responseHandler.BadRequest(CustomErrorCode.IsHighlight, CustomErrorMessage.StoryHighlightDeleteError, ""));
                }
                return Ok(_responseHandler.Success(CustomErrorMessage.StoryHighlightDelete, storyHighLightId));
            }
            catch (Exception ex)
            {
                if (ex is ValidationException vx)
                {
                    return BadRequest(_responseHandler.BadRequest(vx.ErrorCode, vx.Message, vx.Errors));
                }
                else
                {
                    return BadRequest(_responseHandler.BadRequest(CustomErrorCode.IsHighlight, ex.Message, ""));
                }
            }
        }

        /// <summary>
        /// Retrieves a paginated list of highlights for a user based on the provided user ID.
        /// </summary>
        /// <param name="model">Request DTO containing the user ID and pagination parameters.</param>
        /// <returns>
        /// An ActionResult representing the result of the operation.
        /// If successful, returns HTTP 200 (OK) with a pagination response model containing the list of highlights.
        /// If validation fails or an error occurs, returns HTTP 400 (Bad Request) with an error message.
        /// </returns>
        [HttpPost("HighLightListByUserId")]
        [Authorize]
        public async Task<ActionResult> GetHighLightListByUserId([FromBody] RequestDTO<UserIdRequestDTO> model)
        {
            try
            {
                List<ValidationError> errors = _validationService.ValidateGetUserById(model.Model.UserId);
                if (errors.Any())
                {
                    return BadRequest(_responseHandler.BadRequest(CustomErrorCode.IsValid, CustomErrorMessage.ValidationStory, errors));
                }
                PaginationResponceModel<HighlightDTO> data = await _storyService.GetHighLightListByUserId(model);
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
    }
}
