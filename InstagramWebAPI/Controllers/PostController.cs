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
        private readonly Helper _helper;


        public PostController(IValidationService validationService, ResponseHandler responseHandler, IPostService postService, Helper helper)
        {
            _validationService = validationService;
            _responseHandler = responseHandler;
            _postService = postService;
            _helper = helper;
        }

        /// <summary>
        /// Creates a new post asynchronously.
        /// </summary>
        /// <param name="model">The data transfer object containing post creation details.</param>
        /// <returns>An <see cref="ActionResult{T}"/> representing the result of the post creation operation.</returns>
        [HttpPost("CreatePostAsync")]
        [Authorize]
        public async Task<ActionResult> CreatePostAsync([FromForm] CreatePostDTO model)
        {
            try
            {
                List<ValidationError> errors = _validationService.ValidateCreatePost(model);
                if (errors.Any())
                {
                    return BadRequest(_responseHandler.BadRequest(CustomErrorCode.IsValid, CustomErrorMessage.ValidationPost, errors));
                }
                PostResponseDTO responseDTO = await _postService.CreatePostAsync(model);
                if (responseDTO == null)
                {
                    return BadRequest(_responseHandler.BadRequest(CustomErrorCode.IsPOst, CustomErrorMessage.PostError, ""));
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
                    return BadRequest(_responseHandler.BadRequest(CustomErrorCode.IsPOst, ex.Message, ""));
                }
            }
        }
        [HttpPost("GetPostById")]
        [Authorize]
        public async Task<ActionResult> GetPostByIdAsync([FromBody] long postId,string postType)
        {
            try
            {
                List<ValidationError> errors = _validationService.ValidatePostById(postId,postType);
                if (errors.Any())
                {
                    return BadRequest(_responseHandler.BadRequest(CustomErrorCode.IsValid, CustomErrorMessage.ValidationPost, ""));
                }
                PostResponseDTO responseDTO = await _postService.GetPostById(postId,postType);
                if (responseDTO == null)
                {
                    return BadRequest(_responseHandler.BadRequest(CustomErrorCode.IsGetPOst, CustomErrorMessage.PostGetError, postId));
                }
                return Ok(_responseHandler.Success(CustomErrorMessage.GetPost, responseDTO));
            }
            catch (Exception ex)
            {
                if (ex is ValidationException vx)
                {
                    return BadRequest(_responseHandler.BadRequest(vx.ErrorCode, vx.Message, vx.Errors));
                }
                else
                {
                    return BadRequest(_responseHandler.BadRequest(CustomErrorCode.IsGetPOst, ex.Message, ""));
                }
            }
        }

        /// <summary>
        /// Retrieves a list of posts and reels based on the provided request data asynchronously.
        /// </summary>
        /// <param name="model">The request data object containing parameters for retrieving post and reel lists.</param>
        /// <returns>
        /// An asynchronous task that returns an <see cref="ActionResult{T}"/> where T is <see cref="ResponseModel"/>.
        /// </returns>
        [HttpPost("PostAndReelListById")]
        [Authorize]
        public async Task<ActionResult> GetPostAndReelListByIdAsync([FromBody] RequestDTO<PostListRequestDTO> model)
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
                    return BadRequest(_responseHandler.BadRequest(CustomErrorCode.IsGetLIst, CustomErrorMessage.GetFollowerList, ""));
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
                    return BadRequest(_responseHandler.BadRequest(CustomErrorCode.IsGetLIst, ex.Message, ""));
                }
            }
        }

        /// <summary>
        /// Deletes a post asynchronously based on the provided post ID.
        /// </summary>
        /// <param name="postId">The unique identifier of the post to delete.</param>
        /// <returns>
        /// An asynchronous task that returns an <see cref="ActionResult{T}"/> where T is <see cref="ResponseModel"/>.
        /// If successful, returns an <see cref="ActionResult"/> with a success message indicating the post was deleted.
        /// </returns>
        [HttpPost("DeletePost")]
        [Authorize]
        public async Task<ActionResult> DetelePostAsync(long postId)
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
                    return BadRequest(_responseHandler.BadRequest(CustomErrorCode.IsPostDelete, CustomErrorMessage.PostDeleteError, ""));
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
                    return BadRequest(_responseHandler.BadRequest(CustomErrorCode.IsPostDelete, ex.Message, ""));
                }
            }
        }

        /// <summary>
        /// Likes or unlikes a post asynchronously based on the provided data.
        /// </summary>
        /// <param name="model">The data object containing user ID and post ID for the like operation.</param>
        /// <returns>
        /// An asynchronous task that returns an <see cref="ActionResult{T}"/> where T is <see cref="ResponseModel"/>.
        /// If successful, returns an <see cref="ActionResult"/> with a success message indicating the post was liked or unliked.
        /// </returns>
        [HttpPost("LikeAndUnlikePost")]
        [Authorize]
        public async Task<ActionResult> LikeAndUnlikePostAsync(LikePostDTO model)
        {
            try
            {
                List<ValidationError> errors = _validationService.ValidateLikePost(model.UserId, model.PostId);
                if (errors.Any())
                {
                    return BadRequest(_responseHandler.BadRequest(CustomErrorCode.IsValid, CustomErrorMessage.ValidationPost, errors));
                }
                bool IsLike = await _postService.LikeAndUnlikePostAsync(model);
                if (!IsLike)
                {
                    return BadRequest(_responseHandler.BadRequest(CustomErrorCode.IsPostLIke, CustomErrorMessage.PostLikeError, ""));
                }
                return Ok(_responseHandler.Success(CustomErrorMessage.PostLike, model.PostId));
            }
            catch (Exception ex)
            {
                if (ex is ValidationException vx)
                {
                    return BadRequest(_responseHandler.BadRequest(vx.ErrorCode, vx.Message, vx.Errors));
                }
                else
                {
                    return BadRequest(_responseHandler.BadRequest(CustomErrorCode.IsPostLIke, ex.Message, ""));
                }
            }
        }

        /// <summary>
        /// Adds a comment to a post asynchronously based on the provided data.
        /// </summary>
        /// <param name="model">The data object containing user ID, post ID, and comment content.</param>
        /// <returns>
        /// An asynchronous task that returns an <see cref="ActionResult{T}"/> where T is <see cref="ResponseModel"/>.
        /// If successful, returns an <see cref="ActionResult"/> with a success message indicating the comment was added.
        /// </returns>
        [HttpPost("CommentPost")]
        [Authorize]
        public async Task<ActionResult> CommentPostAsync(CommentPostDTO model)
        {
            try
            {
                List<ValidationError> errors = _validationService.ValidateCommentPost(model);
                if (errors.Any())
                {
                    return BadRequest(_responseHandler.BadRequest(CustomErrorCode.IsValid, CustomErrorMessage.ValidationPost, errors));
                }
                bool isComment = await _postService.CommentPostAsync(model);
               
                return Ok(_responseHandler.Success(CustomErrorMessage.PostComment, model));
            }
            catch (Exception ex)
            {
                if (ex is ValidationException vx)
                {
                    return BadRequest(_responseHandler.BadRequest(vx.ErrorCode, vx.Message, vx.Errors));
                }
                else
                {
                    return BadRequest(_responseHandler.BadRequest(CustomErrorCode.IsPostComment, ex.Message, ""));
                }
            }
        }

        /// <summary>
        /// Deletes a comment from a post asynchronously based on the provided comment ID.
        /// </summary>
        /// <param name="commentId">The unique identifier of the comment to delete.</param>
        /// <returns>
        /// An asynchronous task that returns an <see cref="ActionResult{T}"/> where T is <see cref="ResponseModel"/>.
        /// If successful, returns an <see cref="ActionResult"/> with a success message indicating the comment was deleted.
        /// </returns>
        [HttpPost("DeletePostComment")]
        [Authorize]
        public async Task<ActionResult> DetelePostCommentAsync(long commentId)
        {
            try
            {
                List<ValidationError> errors = _validationService.ValidateCommentId(commentId);
                if (errors.Any())
                {
                    return BadRequest(_responseHandler.BadRequest(CustomErrorCode.IsValid, CustomErrorMessage.ValidationPost, errors));
                }
                bool isDeleted = await _postService.DetelePostCommentAsync(commentId);
                if (!isDeleted)
                {
                    return BadRequest(_responseHandler.BadRequest(CustomErrorCode.IsPostComment, CustomErrorMessage.PostCommentError, ""));
                }
                return Ok(_responseHandler.Success(CustomErrorMessage.PostCommentDelete, commentId));
            }
            catch (Exception ex)
            {
                if (ex is ValidationException vx)
                {
                    return BadRequest(_responseHandler.BadRequest(vx.ErrorCode, vx.Message, vx.Errors));
                }
                else
                {
                    return BadRequest(_responseHandler.BadRequest(CustomErrorCode.IsPostDelete, ex.Message, ""));
                }
            }
        }
    }
}
