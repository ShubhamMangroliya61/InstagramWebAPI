using DataAccess.CustomModel;
using InstagramWebAPI.Common;
using InstagramWebAPI.DAL.Models;
using InstagramWebAPI.DTO;
using InstagramWebAPI.Helpers;
using InstagramWebAPI.Interface;
using InstagramWebAPI.Utils;
using Microsoft.EntityFrameworkCore;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace InstagramWebAPI.BLL
{
    public class UserService : IUserService
    {
        public readonly ApplicationDbContext _dbcontext;
        public readonly Helper _helper;
        public UserService(ApplicationDbContext db,Helper helper)
        {
            _helper = helper;
            _dbcontext = db;
        }

        /// <summary>
        /// Uploads a profile photo for a user asynchronously.
        /// </summary>
        /// <param name="model">The model containing UserId and the profile photo to upload.</param>
        /// <returns>A ProfilePhotoResponseDTO containing the uploaded photo details.</returns>
        public async Task<ProfilePhotoResponseDTO> UploadProfilePhotoAsync(IFormFile ProfilePhoto,long userId)
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


            IFormFile file = ProfilePhoto;

            // Delete the old profile photo file if it exists
            if (!string.IsNullOrEmpty(user.ProfilePictureUrl) && System.IO.File.Exists(Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", user.ProfilePictureUrl)))
            {
                System.IO.File.Delete(Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", user.ProfilePictureUrl));
            }

            if (ProfilePhoto != null)
            {
                string userID = userId.ToString();

                string path = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "content", "User", userID, "ProfilePhoto");

                if (!Directory.Exists(path))
                {
                    Directory.CreateDirectory(path);
                }

                string fileName = Guid.NewGuid().ToString() + Path.GetExtension(file.FileName);
                string filePath = Path.Combine(path, fileName);

                using (var fileStream = new FileStream(filePath, FileMode.Create))
                {
                    await file.CopyToAsync(fileStream);
                }

                user.ProfilePictureUrl = Path.Combine("content", "User", userID, "ProfilePhoto", fileName);
                user.ProfilePictureName = fileName;
            }
            else
            {
                user.ProfilePictureUrl = "";
                user.ProfilePictureName = "";
            }
            user.ModifiedDate=DateTime.Now;
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

            UserDTO userDTO = _helper.UserMapper(user);
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

            int postCount = await _dbcontext.Posts
                .Where(m => m.UserId == userId && m.IsDeleted == false && m.PostTypeId == 4)
                .CountAsync();
            return new CountResponseDTO
            {
                FolloweCount = followerCount,
                FollowingCount = followingCount,
                PostCount = postCount
            };
        }

        /// <summary>
        /// Gets the content type based on the file extension.
        /// </summary>
        /// <param name="fileExtension">The file extension (without the dot).</param>
        /// <returns>The content type string corresponding to the provided file extension.</returns>
        public string GetContentType(string fileExtension)
        {
            return fileExtension switch
            {
                "jpg" or "jpeg" => "image/jpeg",
                "png" => "image/png",
                "mp4" => "video/mp4",
                _ => "application/octet-stream",
            };
        }

        /// <summary>
        /// Retrieves mutual friends with details between the authenticated user and another user.
        /// </summary>
        /// <param name="model">The request model containing the target user's ID.</param>
        /// <returns>A paginated list of mutual friends with their details.</returns>
        public async Task<PaginationResponceModel<MutualFriendDTO>> GetMutualFriendsWithDetailsAsync(RequestDTO<UserIdRequestDTO> model)
        {
            long fromUserId = _helper.GetUserIdClaim();
            IQueryable<MutualFriendDTO> mutualFriends = _dbcontext.Requests
                .Where( r1 => r1.FromUserId == fromUserId && r1.IsAccepted && !r1.IsDeleted &&
                             _dbcontext.Requests
                                .Any(r2 => r2.FromUserId == r1.ToUserId && r2.IsAccepted && !r2.IsDeleted && r2.ToUserId == model.Model.UserId))
                .Join(_dbcontext.Users,
                      r1 => r1.ToUserId,
                      user => user.UserId,
                      (r1, user) => new MutualFriendDTO
                      {
                          UserId = user.UserId,
                          UserName = user.UserName,
                          ProfilePictureName = user.ProfilePictureName ?? string.Empty
                      }) ;

            List<MutualFriendDTO> result = await mutualFriends
               .Skip((model.PageNumber - 1) * model.PageSize)
            .Take(model.PageSize)
               .ToListAsync();

            int totalRecords = await mutualFriends.CountAsync();
            int requiredPages = (int)Math.Ceiling((decimal)totalRecords / model.PageSize);
            return new PaginationResponceModel<MutualFriendDTO>
            {
                Totalrecord = totalRecords,
                PageSize = model.PageSize,
                PageNumber = model.PageNumber,
                RequirdPage = requiredPages,
                Record = result
            };
        }
    }
}
