using DataAccess.CustomModel;
using InstagramWebAPI.Common;
using InstagramWebAPI.DAL.Models;
using InstagramWebAPI.DTO;
using InstagramWebAPI.Helpers;
using InstagramWebAPI.Interface;
using InstagramWebAPI.Utils;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel;
using static InstagramWebAPI.Utils.Enum;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace InstagramWebAPI.BLL
{
    public class PostService : IPostService
    {
        public readonly ApplicationDbContext _dbcontext;
        private readonly Helper _helper;

        public PostService(ApplicationDbContext db, Helper helper)
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
                    PostMapping postMapping = new()
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
                await _dbcontext.SaveChangesAsync();
            }

            // Prepare response DTO
            PostResponseDTO responseDTO = new()
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
        /// Retrieves a post by its ID and type asynchronously.
        /// </summary>
        /// <param name="postId">The ID of the post to retrieve.</param>
        /// <param name="postType">The type of the post ("Post" or "Reel").</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains a PostResponseDTO which includes the post details such as post ID, user information, caption, location, media, likes, and comments.</returns>
        /// <exception cref="ValidationException">Thrown when the post is not found.</exception>
        public async Task<PostResponseDTO> GetPostById(long postId, string postType)
        {
            Post post = await _dbcontext.Posts
                .Include(p => p.PostMappings)
                    .ThenInclude(pm => pm.MediaType)
                .Include(p => p.Likes)
                    .ThenInclude(l => l.User)
                .Include(p => p.Comments)
                    .ThenInclude(c => c.User)
                .Include(p => p.User)
                .FirstOrDefaultAsync(p => p.PostId == postId && !p.IsDeleted &&
                                          (postType == "Post" ? p.PostTypeId == 4 : p.PostTypeId == 3))
                ??
                throw new ValidationException(CustomErrorMessage.NullPostId, CustomErrorCode.NullPostId, new List<ValidationError>
                {
                    new ValidationError
                    {
                     message = CustomErrorMessage.NullPostId,
                    reference = "postid",
                    parameter = "postid",
                    errorCode = CustomErrorCode.NullPostId
                    }
                });

            return new PostResponseDTO
            {
                PostId = post.PostId,
                UserId = post.UserId,
                UserName = post.User.UserName,
                ProfilePhotoName = post.User.ProfilePictureName,
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
                PostLikes = post.Likes.Where(l => l.IsDeleted == false).Select(l => new PostLike
                {
                    LikeId = l.LikeId,
                    UserId = l.UserId,
                    Avtar = l.User.ProfilePictureName,
                    UserName = l.User.UserName
                }).ToList(),
                PostComments = post.Comments.Where(l => l.IsDeleted == false).Select(c => new PostComment
                {
                    CommentId = c.CommentId,
                    UserId = c.UserId,
                    CommentText = c.CommentText,
                    Avtar = c.User.ProfilePictureName,
                    UserName = c.User.UserName
                }).ToList()
            };
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
                .Include(m => m.Likes)
                .Include(m => m.PostMappings).Include(m => m.Comments).Include(m => m.User)
                .Where(m => m.IsDeleted == false && (model.Model.PostType == "Post" ? m.PostTypeId == 4 : m.PostTypeId == 3) && m.UserId == model.Model.UserId)
                .OrderByDescending(p => p.CreatedDate)
                .Select(post => new PostResponseDTO
                {
                    PostId = post.PostId,
                    UserId = post.UserId,
                    UserName = post.User.UserName,
                    ProfilePhotoName = post.User.ProfilePictureName,
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
                    PostLikes = post.Likes.Where(l => l.IsDeleted == false).Select(l => new PostLike
                    {
                        LikeId = l.LikeId,
                        UserId = l.UserId,
                        Avtar = l.User.ProfilePictureName,
                        UserName = l.User.UserName
                    }).ToList(),
                    PostComments = post.Comments.Where(l => l.IsDeleted == false).Select(c => new PostComment
                    {
                        CommentId = c.CommentId,
                        UserId = c.UserId,
                        CommentText = c.CommentText,
                        Avtar = c.User.ProfilePictureName,
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
            Like obj = like ?? new();

            obj.UserId = model.UserId;
            obj.PostId = model.PostId;
            obj.IsLike = model.IsLike;

            if (like != null)
            {
                like.IsDeleted = !model.IsLike;
                like.ModifiedDate = DateTime.Now;

                _dbcontext.Likes.Update(like);
            }
            else
            {
                obj.CreatedDate = DateTime.Now;
                await _dbcontext.Likes.AddAsync(obj);
            }
            await _dbcontext.SaveChangesAsync();

            long toUserId = _dbcontext.Posts.FirstOrDefaultAsync(m => m.PostId == model.PostId && !m.IsDeleted).Result?.UserId ?? 0;
            if (toUserId != model.UserId)
            {
                await _helper.CreateNotification(new NotificationDTO()
                {
                    FromUserId = model.UserId,
                    ToUserId = toUserId,
                    NotificationType = NotificationType.PostLiked,
                    NotificationTypeId = NotificationTypeId.LikeId,
                    Id = obj.LikeId,
                    IsDeleted = obj.IsDeleted,
                    PostId = model.PostId,
                });
            }
            return true;
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

            long toUserId = _dbcontext.Posts.FirstOrDefaultAsync(m => m.PostId == model.PostId && !m.IsDeleted).Result?.UserId ?? 0;
            if (toUserId != model.UserId)
            {
                await _helper.CreateNotification(new NotificationDTO()
                {
                    FromUserId = model.UserId,
                    ToUserId = toUserId,
                    NotificationType = NotificationType.PostCommented,
                    NotificationTypeId = NotificationTypeId.CommentId,
                    Id = comment.CommentId,
                    IsDeleted = comment.IsDeleted,
                    PostId = model.PostId,
                });
            }
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

                await _helper.CreateNotification(new NotificationDTO()
                {
                    FromUserId = comment.UserId,
                    ToUserId = _dbcontext.Posts.FirstOrDefaultAsync(m => m.PostId == comment.PostId && !m.IsDeleted).Result?.UserId ?? 0,
                    NotificationType = NotificationType.PostCommented,
                    NotificationTypeId = NotificationTypeId.CommentId,
                    Id = comment.CommentId,
                    IsDeleted = comment.IsDeleted,
                    PostId = comment.PostId,
                });
                return true;
            }
            return false;
        }

        /// <summary>
        /// Retrieves a paginated list of posts for the current user based on accepted requests and non-private users asynchronously.
        /// </summary>
        /// <param name="model">The pagination request details, including page number and page size.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains a PaginationResponceModel of PostResponseDTO which includes total records, page size, page number, required pages, and a list of post response DTOs.</returns>
        public async Task<PaginationResponceModel<PostResponseDTO>> GetPostListByUserIdAsync(RequestDTO<PostRequestDTO> model)
        {
            long userId = _helper.GetUserIdClaim();

            List<long> requestUserIds = await _dbcontext.Requests
                 .Where(m => m.FromUserId == userId && m.IsDeleted == false && m.IsAccepted == true)
                 .Select(m => m.ToUserId)
                 .ToListAsync();

            List<long> userUserIds = await _dbcontext.Users
                .Where(u => u.IsDeleted == false && u.IsPrivate == false)
                .Select(m => m.UserId)
                .ToListAsync();

            List<long> combinedUserIds = requestUserIds.Concat(userUserIds).Distinct().ToList();

            IQueryable<PostResponseDTO> posts = _dbcontext.Posts
                .Include(m => m.Likes)
                .Include(m => m.PostMappings)
                .Include(m => m.Comments)
                .Include(m => m.User)
                .Where(m => !m.IsDeleted && combinedUserIds.Contains(m.UserId) && (string.IsNullOrWhiteSpace(model.Model.PostType) || model.Model.PostType == "Post" ? m.PostTypeId == 4 : m.PostTypeId == 3 ))
                .OrderByDescending(p => p.CreatedDate)
                .Select(post => new PostResponseDTO
                {
                    PostId = post.PostId,
                    UserId = post.UserId,
                    UserName = post.User.UserName,
                    ProfilePhotoName = post.User.ProfilePictureName,
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
                    PostLikes = post.Likes.Where(l => !l.IsDeleted).Select(l => new PostLike
                    {
                        LikeId = l.LikeId,
                        UserId = l.UserId,
                        Avtar = l.User.ProfilePictureName,
                        UserName = l.User.UserName
                    }).ToList(),
                    PostComments = post.Comments.Where(l => !l.IsDeleted).Select(c => new PostComment
                    {
                        CommentId = c.CommentId,
                        UserId = c.UserId,
                        CommentText = c.CommentText,
                        Avtar = c.User.ProfilePictureName,
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
        /// Inserts or updates a collection asynchronously based on the provided collection request details.
        /// </summary>
        /// <param name="model">The collection request details, including collection ID and collection name.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains the upserted CollectionDTO which includes the collection ID and collection name.</returns>
        public async Task<CollectionDTO> UpsertCollectionAsync(CollectionRequestDTO model)
        {
            long userId = _helper.GetUserIdClaim();
            Collection collection = await _dbcontext.Collections.FirstOrDefaultAsync(m => m.CollectionId == model.CollectionId && m.UserId == userId && m.IsDeleted == false) ?? new();

            collection.CollectionName = model.CollectionName ?? string.Empty;
            collection.UserId = userId;

            if (model.CollectionId > 0)
            {
                collection.ModifiedDate = DateTime.Now;
                _dbcontext.Collections.Update(collection);
                await _dbcontext.SaveChangesAsync();
            }
            else
            {
                collection.CreatedDate = DateTime.Now;
                await _dbcontext.Collections.AddAsync(collection);
                await _dbcontext.SaveChangesAsync();
            }

            return new CollectionDTO()
            {
                CollectionId = collection.CollectionId,
                CollectionName = collection.CollectionName,
            };
        }

        /// <summary>
        /// Deletes a collection asynchronously by marking it as deleted based on the provided collection ID.
        /// </summary>
        /// <param name="collectionId">The ID of the collection to delete.</param>
        /// <returns>A task that represents the asynchronous operation. The task result is a boolean indicating whether the collection was successfully marked as deleted.</returns>
        public async Task<bool> DeteleCollectionAsync(long colletionId)
        {
            long userId = _helper.GetUserIdClaim();
            Collection? collection = await _dbcontext.Collections.FirstOrDefaultAsync(m => m.CollectionId == colletionId && m.UserId == userId && m.IsDeleted == false);

            if (collection != null)
            {
                collection.IsDeleted = true;
                collection.ModifiedDate = DateTime.UtcNow;

                _dbcontext.Collections.Update(collection);
                await _dbcontext.SaveChangesAsync();
                return true;
            }
            return false;
        }

        /// <summary>
        /// Adds a post to a collection asynchronously based on the provided collection ID and post ID.
        /// </summary>
        /// <param name="collectionId">The ID of the collection to which the post will be added.</param>
        /// <param name="postId">The ID of the post to be added to the collection.</param>
        /// <returns>A task that represents the asynchronous operation. The task result is a boolean indicating whether the post was successfully added to the collection.</returns>
        public async Task<bool> AddPostCollectionAsync(long collectionId, long postId)
        {
            PostCollection data = new()
            {
                PostId = postId,
                CollectionId = collectionId,
                CreatedDate = DateTime.Now,
            };
            await _dbcontext.PostCollections.AddAsync(data);
            await _dbcontext.SaveChangesAsync();
            return true;
        }

        /// <summary>
        /// Deletes a post from a collection asynchronously by marking it as deleted based on the provided post collection ID.
        /// </summary>
        /// <param name="postCollectionId">The ID of the post collection to delete.</param>
        /// <returns>A task that represents the asynchronous operation. The task result is a boolean indicating whether the post collection was successfully marked as deleted.</returns>
        public async Task<bool> DeletePostCollectionAsync(long postCollectionId)
        {
            PostCollection? postCollection = await _dbcontext.PostCollections.FirstOrDefaultAsync(m => m.PostCollectionId == postCollectionId && m.IsDeleted == false);

            if (postCollection != null)
            {
                postCollection.IsDeleted = true;
                postCollection.ModifiedDate = DateTime.UtcNow;

                _dbcontext.PostCollections.Update(postCollection);
                await _dbcontext.SaveChangesAsync();
                return true;
            }
            return false;
        }

        /// <summary>
        /// Retrieves a paginated list of collections for a specified user asynchronously.
        /// </summary>
        /// <param name="model">The request details, including the user ID, page number, and page size.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains a PaginationResponceModel of CollectionDTO which includes total records, page size, page number, required pages, and a list of collection DTOs.</returns>
        public async Task<PaginationResponceModel<CollectionDTO>> GetcollectionListByUserId(RequestDTO<UserIdRequestDTO> model)
        {
            IQueryable<CollectionDTO> data = _dbcontext.Collections
                        .Include(m => m.PostCollections).ThenInclude(m => m.Post)
                        .Where(m => m.UserId == model.Model.UserId)
                        .Select(m => new CollectionDTO
                        {
                            CollectionId = m.CollectionId,
                            CollectionName = m.CollectionName,
                            PostCollectionList = m.PostCollections.Select(s => new PostCollectionList
                            {
                                PostCollectionId = s.PostCollectionId,
                                PostId = s.PostId,
                                CreatedDate = s.Post.CreatedDate
                            }).ToList()
                        });

            int totalRecords = await data.CountAsync();
            int requiredPages = (int)Math.Ceiling((decimal)totalRecords / model.PageSize);

            // Paginate the data
            List<CollectionDTO> records = await data
                .Skip((model.PageNumber - 1) * model.PageSize)
                .Take(model.PageSize)
                .ToListAsync();

            return new PaginationResponceModel<CollectionDTO>
            {
                Totalrecord = totalRecords,
                PageSize = model.PageSize,
                PageNumber = model.PageNumber,
                RequirdPage = requiredPages,
                Record = records,
            };
        }
    }
}
