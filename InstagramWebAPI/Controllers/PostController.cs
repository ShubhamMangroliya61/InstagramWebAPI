using DataAccess.CustomModel;
using InstagramWebAPI.Common;
using InstagramWebAPI.DTO;
using InstagramWebAPI.Helpers;
using InstagramWebAPI.Interface;
using InstagramWebAPI.Utils;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace InstagramWebAPI.Controllers
{
    [Route("api/Post")]
    [ApiController]
    public class PostController : ControllerBase
    {
        private readonly IValidationService _validationService;
        private readonly IPostService _postService;
        private readonly ResponseHandler _responseHandler;

        public PostController(IValidationService validationService, ResponseHandler responseHandler, IPostService postService)
        {
            _validationService = validationService;
            _responseHandler = responseHandler;
            _postService = postService;
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
                PostResponseDTO responseDTO = await _postService.CreatePostAsync(model);
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

        [HttpPost("PostAndReelListById")]
        public async Task<ActionResult<ResponseModel>> GetPostAndReelListByIdAsync([FromBody] RequestDTO<PostListRequestDTO> model)
        {
            try
            {
                List<ValidationError> errors = _validationService.ValidatePostList(model);
                if (errors.Any())
                {
                    return BadRequest(_responseHandler.BadRequest(CustomErrorCode.IsValid, CustomErrorMessage.ValidationPost, errors));
                }
                PaginationResponceModel<PostResponseDTO> data = await _postService.GetPostsListByIdAsync(model);
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
                bool isDeleted = await _postService.DetelePostAsync(postId);
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
                    return BadRequest(_responseHandler.BadRequest(CustomErrorCode.IsPostDelete, ex.Message, postId));
                }
            }
        }

        [HttpPost("LikeAndUnlikePost")]
        public async Task<ActionResult<ResponseModel>> LikeAndUnlikePostAsync(LikePostDTO model)
        {
            try
            {
                List<ValidationError> errors = _validationService.ValidateLikePost(model.userId, model.postId);
                if (errors.Any())
                {
                    return BadRequest(_responseHandler.BadRequest(CustomErrorCode.IsValid, CustomErrorMessage.ValidationPost, errors));
                }
                bool IsLike = await _postService.LikeAndUnlikePostAsync(model);
                if (!IsLike)
                {
                    return BadRequest(_responseHandler.BadRequest(CustomErrorCode.IsPostLIke, CustomErrorMessage.PostLikeError, errors));
                }
                return Ok(_responseHandler.Success(CustomErrorMessage.PostLike, model.postId));
            }
            catch (Exception ex)
            {
                if (ex is ValidationException vx)
                {
                    return BadRequest(_responseHandler.BadRequest(vx.ErrorCode, vx.Message, vx.Errors));
                }
                else
                {
                    return BadRequest(_responseHandler.BadRequest(CustomErrorCode.IsPostLIke, ex.Message, model.postId));
                }
            }
        }

       

    }
}
