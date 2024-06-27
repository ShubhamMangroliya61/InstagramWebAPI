using DataAccess.CustomModel;
using InstagramWebAPI.Common;
using InstagramWebAPI.DAL.Models;
using InstagramWebAPI.DTO;
using InstagramWebAPI.Interface;
using InstagramWebAPI.Utils;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace InstagramWebAPI.BLL
{
    public class UserService : IUserService
    {
        public readonly ApplicationDbContext _dbcontext;
        public readonly IJWTService _jWTService;


        public UserService(ApplicationDbContext db, IConfiguration configuration, IJWTService jWTService)
        {
            _dbcontext = db;
            _jWTService = jWTService;
        }

        /// <summary>
        /// Uploads a profile photo for a user asynchronously.
        /// </summary>
        /// <param name="model">The model containing UserId and the profile photo to upload.</param>
        /// <returns>A ProfilePhotoResponseDTO containing the uploaded photo details.</returns>
        public async Task<ProfilePhotoResponseDTO> UploadProfilePhotoAsync(UploadProfilePhotoDTO model)
        {

            User user = await _dbcontext.Users.FirstOrDefaultAsync(m => m.UserId == model.UserId && m.IsDeleted != true) ??
               throw new ValidationException(CustomErrorMessage.ExitsUser, CustomErrorCode.IsNotExits, new List<ValidationError>
               {
                       new ValidationError
                    {
                        message = CustomErrorMessage.ExitsUser,
                        reference = "UserName",
                        parameter = "UserName",
                        errorCode = CustomErrorCode.IsNotExits
                    }
               });


            IFormFile file = model.ProfilePhoto ??
                  throw new ValidationException(CustomErrorMessage.NullProfilePhoto, CustomErrorCode.NullProfilePhoto, new List<ValidationError>
                   {
                       new ValidationError
                {
                    message = CustomErrorMessage.NullProfilePhoto,
                    reference = "ProfilePhoto",
                    parameter = "ProfilePhoto",
                    errorCode = CustomErrorCode.NullProfilePhoto
                }
                   }); ;

            string userId = model.UserId.ToString();

            string path = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "content", "User", userId, "ProfilePhoto");

            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }

            string fileName = Guid.NewGuid().ToString() + Path.GetExtension(file.FileName);
            string filePath = Path.Combine(path, fileName);

            // Delete the old profile photo file if it exists
            if (!string.IsNullOrEmpty(user.ProfilePictureUrl) && System.IO.File.Exists(Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", user.ProfilePictureUrl)))
            {
                System.IO.File.Delete(Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", user.ProfilePictureUrl));
            }


            using (var fileStream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(fileStream);
            }


            user.ProfilePictureUrl = Path.Combine("content", "User", userId, "ProfilePhoto", fileName);
            user.ProfilePictureName = fileName;


            _dbcontext.Users.Update(user);
            await _dbcontext.SaveChangesAsync();

            ProfilePhotoResponseDTO photoResponseDTO = new()
            {
                ProfilePhotoName = user.ProfilePictureName,
                ProfilePhotoUrl = user.ProfilePictureUrl,
                UserId = user.UserId,
            };
            return photoResponseDTO;
        }

        /// <summary>
        /// Handles the follow request asynchronously between users.
        /// </summary>
        /// <param name="model">The data transfer object containing follow request details.</param>
        /// <returns>A boolean indicating whether the follow request operation was successful.</returns>
        public async Task<bool> FollowRequestAsync(FollowRequestDTO model)
        {
            try
            {
                if (!await _dbcontext.Users.AnyAsync(m => m.UserId == model.ToUserId) ||
                    !await _dbcontext.Users.AnyAsync(m => m.UserId == model.FromUserId))
                {
                    throw new ValidationException(CustomErrorMessage.ExitsUser, CustomErrorCode.IsNotExits, new List<ValidationError>
                {
                        new ValidationError
                    {
                        message = CustomErrorMessage.ExitsUser,
                        reference = "UserName",
                        parameter = "UserName",
                        errorCode = CustomErrorCode.IsNotExits
                    }
                });
                }

                Request data = await _dbcontext.Requests.FirstOrDefaultAsync(m => m.FromUserId == model.FromUserId && m.ToUserId == model.ToUserId)
                             ?? new();

                data.FromUserId = model.FromUserId;
                data.ToUserId = model.ToUserId;

                var user = _dbcontext.Users.FirstOrDefault(m => m.UserId == model.ToUserId && m.IsDeleted == false);
                var isPublicUser = true;

                if (user != null && user.IsPrivate == true)
                {
                    isPublicUser = false;
                }

                if (data.RequestId > 0)
                {
                    if (isPublicUser)
                    {
                        data.ModifiedDate = DateTime.Now;
                        data.IsDeleted = !(data.IsDeleted);
                        if (data.IsDeleted == true)
                        {
                            data.IsAccepted = false;
                        }
                        else
                        {
                            data.IsAccepted = true;
                        }
                    }
                    else
                    {
                        data.ModifiedDate = DateTime.Now;
                        data.IsDeleted = !(data.IsDeleted);
                        if (data.IsDeleted == true)
                        {
                            data.IsAccepted = false;
                        }
                    }


                    _dbcontext.Requests.Update(data);
                }
                else
                {
                    if (isPublicUser)
                    {
                        data.CreatedDate = DateTime.Now;
                        data.IsAccepted = true;
                    }
                    else
                    {
                        data.CreatedDate = DateTime.Now;
                    }

                    await _dbcontext.Requests.AddAsync(data);
                }
                await _dbcontext.SaveChangesAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Retrieves a paginated list of followers or followings of a user.
        /// </summary>
        /// <param name="model">The request DTO containing filtering and pagination parameters.</param>
        /// <returns>A pagination response model containing the list of users (followers or followings).</returns>
        public async Task<PaginationResponceModel<UserDTO>> GetFollowerORFollowingListAsync(RequestDTO<FollowerListRequestDTO> model)
        {
            IQueryable<Request> query = model.Model.FollowerOrFollowing switch
            {
                "Follower" => _dbcontext.Requests.Include(m => m.ToUser).Where(m => m.ToUserId == model.Model.UserId && m.IsAccepted != false && m.IsDeleted != true),
                "Following" => _dbcontext.Requests.Include(m => m.ToUser).Where(m => m.FromUserId == model.Model.UserId && m.IsAccepted != false && m.IsDeleted != true),
                _ => throw new ArgumentException("Invalid FollowerOrFollowing value")
            };

            IQueryable<UserDTO> Data = query
                .Select(m => new UserDTO
                {
                    UserId = m.ToUser.UserId,
                    UserName = m.ToUser.UserName,
                    Email = m.ToUser.Email,
                    Name = m.ToUser.Name,
                    Bio = m.ToUser.Bio,
                    Link = m.ToUser.Link,
                    Gender = m.ToUser.Gender ?? string.Empty,
                    ProfilePictureName = m.ToUser.ProfilePictureName,
                    ProfilePictureUrl = m.ToUser.ProfilePictureUrl,
                    ContactNumber = m.ToUser.ContactNumber,
                    IsPrivate = m.ToUser.IsPrivate,
                    IsVerified = m.ToUser.IsVerified,
                });

            int totalRecords = await Data.CountAsync();
            int requiredPages = (int)Math.Ceiling((decimal)totalRecords / model.PageSize);

            // Paginate the data
            List<UserDTO> records = await Data
                .Skip((model.PageNumber - 1) * model.PageSize)
                .Take(model.PageSize)
                .ToListAsync();

            return new PaginationResponceModel<UserDTO>
            {
                Totalrecord = totalRecords,
                PageSize = model.PageSize,
                PageNumber = model.PageNumber,
                RequirdPage = requiredPages,
                Record = records,
            };
        }

        /// <summary>
        /// Retrieves a paginated list of requests made to a user.
        /// </summary>
        /// <param name="model">The request DTO containing user ID and pagination parameters.</param>
        /// <returns>A pagination response model containing the list of
        public async Task<PaginationResponceModel<RequestListResponseDTO>> GetRequestListByIdAsync(RequestDTO<FollowRequestDTO> model)
        {
            IQueryable<RequestListResponseDTO> data = _dbcontext.Requests
                .Include(m => m.ToUser)
                .Where(m => m.ToUserId == model.Model.UserId && m.IsDeleted == false && m.IsAccepted == true)
                .OrderByDescending(r => r.CreatedDate)
                .Select(r => new RequestListResponseDTO
                {
                    RequestId = r.RequestId,
                    User = new UserDTO
                    {
                        UserId = r.ToUser.UserId,
                        UserName = r.ToUser.UserName,
                        Email = r.ToUser.Email,
                        Name = r.ToUser.Name,
                        Bio = r.ToUser.Bio,
                        Link = r.ToUser.Link,
                        Gender = r.ToUser.Gender ?? string.Empty,
                        ProfilePictureName = r.ToUser.ProfilePictureName ?? string.Empty,
                        ProfilePictureUrl = r.ToUser.ProfilePictureUrl ?? string.Empty,
                        ContactNumber = r.ToUser.ContactNumber ?? string.Empty,
                        IsPrivate = r.ToUser.IsPrivate,
                        IsVerified = r.ToUser.IsVerified
                    }
                });

            List<RequestListResponseDTO> requests = await data
                .Skip((model.PageNumber - 1) * model.PageSize)
                .Take(model.PageSize)
                .ToListAsync();

            int totalRecords = await data.CountAsync();
            int requiredPages = (int)Math.Ceiling((decimal)totalRecords / model.PageSize);
            return new PaginationResponceModel<RequestListResponseDTO>
            {
                Totalrecord = totalRecords,
                PageSize = model.PageSize,
                PageNumber = model.PageNumber,
                RequirdPage = requiredPages,
                Record = requests
            };
        }

        /// <summary>
        /// Retrieves a user by their ID asynchronously.
        /// </summary>
        /// <param name="userId">The ID of the user to retrieve.</param>
        /// <returns>A UserDTO object representing the user.</returns>
        public async Task<UserDTO> GetUserByIdAsync(long userId)
        {
            User user = await _dbcontext.Users.FirstOrDefaultAsync(m => m.UserId == userId && m.IsDeleted != true) ??
               throw new ValidationException(CustomErrorMessage.ExitsUser, CustomErrorCode.IsNotExits, new List<ValidationError>
               {
                       new ValidationError
                    {
                        message = CustomErrorMessage.ExitsUser,
                        reference = "UserName",
                        parameter = "UserName",
                        errorCode = CustomErrorCode.IsNotExits
                    }
               });

            UserDTO userDTO = new()
            {
                UserId = user.UserId,
                UserName = user.UserName,
                Email = user.Email,
                Name = user.Name,
                Bio = user.Bio,
                Link = user.Link,
                Gender = user.Gender ?? string.Empty,
                ProfilePictureName = user.ProfilePictureName ?? string.Empty,
                ProfilePictureUrl = user.ProfilePictureUrl ?? string.Empty,
                ContactNumber = user.ContactNumber ?? string.Empty,
                IsPrivate = user.IsPrivate,
                IsVerified = user.IsVerified
                // Map other properties as needed
            };
            return userDTO;
        }

        /// <summary>
        /// Accepts or cancels a request asynchronously.
        /// </summary>
        /// <param name="requestId">The ID of the request to accept or cancel.</param>
        /// <param name="acceptType">The type of action: "Accept" or "Cancel".</param>
        /// <returns>True if the operation was successful; otherwise, false.</returns>
        public async Task<bool> RequestAcceptOrCancelAsync(long requestId, string acceptType)
        {
            Request request = await _dbcontext.Requests.FirstOrDefaultAsync(m => m.RequestId == requestId && m.IsDeleted == false)
                ?? throw new ValidationException(CustomErrorMessage.ExitsRequest, CustomErrorCode.IsNotRequest, new List<ValidationError>
                   {
                       new ValidationError
                    {
                        message = CustomErrorMessage.ExitsRequest,
                        reference = "UserName",
                        parameter = "UserName",
                        errorCode = CustomErrorCode.IsNotRequest
                    }
                   });

            if (acceptType == "Accept")
            {
                request.IsAccepted = true;
            }
            else if (acceptType == "Cancel")
            {
                request.IsAccepted = false;
                request.IsDeleted = true;
            }
            _dbcontext.Requests.Update(request);
            await _dbcontext.SaveChangesAsync();
            return true;
        }

        /// <summary>
        /// Retrieves the count of followers and followings for a user.
        /// </summary>
        /// <param name="userId">The ID of the user.</param>
        /// <returns>A CountResponseDTO object containing follower and following counts.</returns>
        public async Task<CountResponseDTO> GetFollowerAndFollowingCountByIdAsync(long userId)
        {

            int followerCount = await _dbcontext.Requests
                .Include(r => r.ToUser)
                .Where(r => r.ToUserId == userId && r.IsAccepted == true && r.IsDeleted == false)
                .CountAsync();


            int followingCount = await _dbcontext.Requests
                .Include(r => r.ToUser)
                .Where(r => r.FromUserId == userId && r.IsAccepted == true && r.IsDeleted == false)
                .CountAsync();

            return new CountResponseDTO
            {
                FolloweCount = followerCount,
                FollowingCount = followingCount
            };
        }

        /// <summary>
        /// Creates or updates a post asynchronously.
        /// </summary>
        /// <param name="model">The DTO containing post data.</param>
        /// <returns>A PostResponseDTO object representing the created or updated post.</returns>
        public async Task<PostResponseDTO> CreatePostAsync(CreatePostDTO model)
        {
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
                post.UserId = model.UserId;
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


            var medias = new List<Media>();
            var postMappings = new List<PostMapping>();
            if (model.PostId == 0)
            {
                foreach (var file in model.File)
                {
                    var mediaType = Path.GetExtension(file.FileName).TrimStart('.');
                    string userId = model.UserId.ToString();
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
                    var postMapping = new PostMapping
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
                    var media = new Media
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
            var responseDTO = new PostResponseDTO
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

        public async Task<PaginationResponceModel<PostResponseDTO>> GetPostsByIdAsync(RequestDTO<PostListRequestDTO> model)
        {
            IQueryable<PostResponseDTO> posts = _dbcontext.Posts
                .Include(m => m.PostMappings)
                .ThenInclude(m => m.MediaType)
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
        public async Task<bool> LikePostAsync(long userId,long postId)
        {
            Like? like = await _dbcontext.Likes.FirstOrDefaultAsync(m => m.UserId == userId && m.PostId ==postId && m.IsDeleted == false);
            if (like != null)
            {
                like.IsDeleted = !(like.IsDeleted);
                like.ModifiedDate = DateTime.Now;

                _dbcontext.Likes.Update(like);
                await _dbcontext.SaveChangesAsync();
                return true;
            }
            else
            {
                Like like1 = new()
                {
                    UserId = userId,
                    PostId = postId,
                    IsDeleted = false,
                    CreatedDate = DateTime.Now,
                };
                await _dbcontext.Likes.AddAsync(like1);
                await _dbcontext.SaveChangesAsync();
                return true;
            }
            

        }
    }
}
