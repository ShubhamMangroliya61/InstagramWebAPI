using DataAccess.CustomModel;
using InstagramWebAPI.DAL.Models;
using InstagramWebAPI.DTO;
using InstagramWebAPI.Helpers;
using InstagramWebAPI.Interface;
using Microsoft.EntityFrameworkCore;

namespace InstagramWebAPI.BLL
{
    public class PostService: IPostService
    {
        public readonly ApplicationDbContext _dbcontext;
        private readonly Helper _helper;

        public PostService(ApplicationDbContext db,Helper helper)
        {
            _dbcontext = db;
            _helper = helper;
        }
        /// <summary>
        /// Creates or updates a post asynchronously.
        /// </summary>
        /// <param name="model">The DTO containing post data.</param>
        /// <returns>A PostResponseDTO object representing the created or updated post.</returns>
        public async Task<PostResponseDTO> CreatePostAsync(CreatePostDTO model)
        {
            long UserId = _helper.GetUserIdClaim();
            Post post = await _dbcontext.Posts.FirstOrDefaultAsync(m => m.PostId == model.PostId && m.IsDeleted == false) ?? new();

            post.Caption = model.Caption;
            post.Location = model.Location;

            if (model.PostId > 0)
            {
                post.ModifiedDate = DateTime.Now;
                _dbcontext.Posts.Update(post);
            }
            else
            {
                post.CreatedDate = DateTime.Now;
                post.UserId = UserId;
                if (model.PostType == "Post")
                {
                    post.PostTypeId = 4;
                }
                else if (model.PostType == "Reel")
                {
                    post.PostTypeId = 3;
                }

                await _dbcontext.Posts.AddAsync(post);
            }
            await _dbcontext.SaveChangesAsync();


            List<Media> medias = new();
            List<PostMapping> postMappings = new();
            if (model.PostId == 0)
            {
                foreach (var file in model.File)
                {
                    string mediaType = Path.GetExtension(file.FileName).TrimStart('.');
                    string userId = UserId.ToString();
                    string path = "";

                    if (model.PostType == "Post")
                    {
                        path = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "content", "User", userId, "Post");
                    }
                    else if (model.PostType == "Reel")
                    {
                        path = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "content", "User", userId, "Reel");
                    }

                    if (!Directory.Exists(path))
                    {
                        Directory.CreateDirectory(path ?? string.Empty);
                    }

                    string fileName = Guid.NewGuid().ToString() + Path.GetExtension(file.FileName);
                    string filePath = Path.Combine(path ?? string.Empty, fileName);

                    using (var fileStream = new FileStream(filePath, FileMode.Create))
                    {
                        await file.CopyToAsync(fileStream);
                    }

                    string mediaURL = Path.Combine("content", "User", userId, model.PostType ?? string.Empty, fileName);

                    int mediaTypeId = 0;

                    if (file.ContentType.Contains("image"))
                    {
                        mediaTypeId = 1;
                    }
                    else if (file.ContentType.Contains("video"))
                    {
                        mediaTypeId = 2;
                    }
                    PostMapping postMapping = new ()
                    {
                        PostId = post.PostId,
                        MediaTypeId = mediaTypeId,
                        MediaUrl = mediaURL,
                        MediaName = fileName,
                        CreatedDate = DateTime.Now,
                        ModifiedDate = DateTime.Now,
                    };

                    postMappings.Add(postMapping);
                    await _dbcontext.SaveChangesAsync();
                    Media media = new()
                    {
                        PostMappingId = postMapping.PostMappingId,
                        MediaType = file.ContentType,
                        MediaURL = mediaURL,
                        MediaName = fileName,
                    };
                    medias.Add(media);
                }

                await _dbcontext.PostMappings.AddRangeAsync(postMappings);
                await _dbcontext.SaveChangesAsync(); // Save all mappings in one go
            }

            // Prepare response DTO
            PostResponseDTO responseDTO = new ()
            {
                PostId = post.PostId,
                UserId = post.UserId,
                Caption = post.Caption,
                Location = post.Location,
                PostType = model.PostType,
                Medias = medias
            };
            return responseDTO;
        }

        /// <summary>
        /// Retrieves a paginated list of posts based on the provided request data.
        /// </summary>
        /// <param name="model">The request data object containing parameters for filtering and pagination.</param>
        /// <returns>
        /// A task representing the asynchronous operation that returns a <see cref="PaginationResponceModel{T}"/> where T is <see cref="PostResponseDTO"/>.
        /// The response contains paginated post data including post details, likes, and comments.
        /// </returns>
        public async Task<PaginationResponceModel<PostResponseDTO>> GetPostsListByIdAsync(RequestDTO<PostListRequestDTO> model)
        {
            IQueryable<PostResponseDTO> posts = _dbcontext.Posts
                .Include(m => m.PostMappings)
                .ThenInclude(m => m.MediaType).Include(m=>m.Likes).Include(m=>m.Comments).Include(m=>m.User)
                .Where(m => m.IsDeleted == false && (model.Model.PostType == "Post" ? m.PostTypeId == 4 : m.PostTypeId == 3) && m.UserId == model.Model.UserId)
                .OrderByDescending(p => p.CreatedDate)
                .Select(post => new PostResponseDTO
                {
                    PostId = post.PostId,
                    UserId = post.UserId,
                    Caption = post.Caption,
                    Location = post.Location,
                    PostType = post.PostTypeId == 3 ? "Reel" : "Post",
                    Medias = post.PostMappings.Select(m => new Media
                    {
                        PostMappingId = m.PostMappingId,
                        MediaType = m.MediaTypeId == 1 ? "Images" : "Video",
                        MediaURL = m.MediaUrl,
                        MediaName = m.MediaName
                    }).ToList(),
                     PostLikes= post.Likes.Select(l => new PostLike
                    {
                        LikeId = l.LikeId,
                        UserId = l.UserId,
                        Avtar = l.User.ProfilePictureUrl, 
                        UserName = l.User.UserName 
                    }).ToList(),
                    PostComments = post.Comments.Select(c => new PostComment
                    {
                        CommentId = c.CommentId,
                        UserId = c.UserId,
                        CommentText = c.CommentText,
                        Avtar = c.User.ProfilePictureUrl, 
                        UserName = c.User.UserName 
                    }).ToList()

                });

            int totalRecords = await posts.CountAsync();
            int requiredPages = (int)Math.Ceiling((decimal)totalRecords / model.PageSize);

            List<PostResponseDTO> postResponses = await posts
                .Skip((model.PageNumber - 1) * model.PageSize)
                .Take(model.PageSize)
                .ToListAsync();

            return new PaginationResponceModel<PostResponseDTO>
            {
                Totalrecord = totalRecords,
                PageSize = model.PageSize,
                PageNumber = model.PageNumber,
                RequirdPage = requiredPages,
                Record = postResponses
            };
        }

        /// <summary>
        /// Soft-deletes a post asynchronously based on the provided post ID.
        /// </summary>
        /// <param name="postId">The unique identifier of the post to delete.</param>
        /// <returns>
        /// A task representing the asynchronous operation that returns a boolean indicating whether the post was successfully soft-deleted.
        /// Returns true if the post was found and soft-deleted.
        /// </returns>
        public async Task<bool> DetelePostAsync(long postId)
        {
            Post? post = await _dbcontext.Posts.FirstOrDefaultAsync(m => m.PostId == postId && m.IsDeleted == false);
            if (post != null)
            {
                post.IsDeleted = true;
                post.ModifiedDate = DateTime.Now;

                _dbcontext.Posts.Update(post);
                await _dbcontext.SaveChangesAsync();

                return true;
            }
            return false;
        }

        /// <summary>
        /// Likes or unlikes a post asynchronously based on the provided data in the <see cref="LikePostDTO"/> model.
        /// </summary>
        /// <param name="model">The data transfer object containing user ID, post ID, and like status.</param>
        /// <returns>
        /// A task representing the asynchronous operation that returns a boolean indicating whether the like/unlike operation was successful.
        /// Returns true if the like/unlike operation was successfully applied.
        /// Returns false if the user attempts to unlike a post they have not previously liked.
        /// </returns>
        public async Task<bool> LikeAndUnlikePostAsync(LikePostDTO model)
        {
            Like? like = await _dbcontext.Likes.FirstOrDefaultAsync(m => m.UserId == model.UserId && m.PostId == model.PostId);

            if (like != null)
            {
                if (model.IsLike != like.IsLike)
                {
                    like.IsLike = model.IsLike;
                    like.IsDeleted = !model.IsLike;
                    like.ModifiedDate = DateTime.Now;

                    _dbcontext.Likes.Update(like);
                    await _dbcontext.SaveChangesAsync();
                }
                return true;
            }
            else
            {
                if (model.IsLike)
                {
                    Like newLike = new()
                    {
                        UserId = model.UserId,
                        PostId = model.PostId,
                        IsLike = true,
                        IsDeleted = false,
                        CreatedDate = DateTime.Now,
                    };

                    await _dbcontext.Likes.AddAsync(newLike);
                    await _dbcontext.SaveChangesAsync();
                    return true;
                }
                return false;
            }
        }

        /// <summary>
        /// Adds a comment to a post asynchronously based on the provided data in the <see cref="CommentPostDTO"/> model.
        /// </summary>
        /// <param name="model">The data transfer object containing user ID, post ID, and comment text.</param>
        /// <returns>
        /// A task representing the asynchronous operation that returns a boolean indicating whether the comment was successfully added.
        /// Returns true if the comment was successfully added to the post.
        /// </returns>
        public async Task<bool> CommentPostAsync(CommentPostDTO model)
        {
            Comment comment = new()
            {
                UserId = model.UserId,
                PostId = model.PostId,
                CommentText = model.CommentText,
                CreatedDate = DateTime.Now,
                IsDeleted = false,
            };
            await _dbcontext.Comments.AddAsync(comment);
            await _dbcontext.SaveChangesAsync();  
            return true;
        }

        /// <summary>
        /// Soft-deletes a post comment asynchronously based on the provided comment ID.
        /// </summary>
        /// <param name="commentId">The unique identifier of the comment to delete.</param>
        /// <returns>
        /// A task representing the asynchronous operation that returns a boolean indicating whether the comment was successfully soft-deleted.
        /// Returns true if the comment was found and soft-deleted.
        /// Returns false if no comment with the specified ID was found or if it was already deleted.
        /// </returns>
        public async Task<bool> DetelePostCommentAsync(long commentId)
        {
            Comment? comment = await _dbcontext.Comments.FirstOrDefaultAsync(m => m.CommentId == commentId && m.IsDeleted == false);
            if (comment != null)
            {
                comment.IsDeleted = true;
                comment.ModifiedDate = DateTime.Now;

                _dbcontext.Comments.Update(comment);
                await _dbcontext.SaveChangesAsync();

                return true;
            }
            return false;
        }
    }
}
