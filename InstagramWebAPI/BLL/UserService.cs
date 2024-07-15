using DataAccess.CustomModel;
using InstagramWebAPI.Common;
using InstagramWebAPI.DAL.Models;
using InstagramWebAPI.DTO;
using InstagramWebAPI.Helpers;
using InstagramWebAPI.Interface;
using InstagramWebAPI.Utils;
using Microsoft.EntityFrameworkCore;
using Org.BouncyCastle.Ocsp;
using static InstagramWebAPI.Utils.Enum;
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
                user.ProfilePictureUrl = null;
                user.ProfilePictureName = null;
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
        public async Task<bool> FollowRequestAsync(FollowRequestDTO model,long FromUserId)
         {
            try
            {
                if (!await _dbcontext.Users.AnyAsync(m => m.UserId == model.ToUserId) ||
                    !await _dbcontext.Users.AnyAsync(m => m.UserId == FromUserId))
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

                Request? data = await _dbcontext.Requests.FirstOrDefaultAsync(m => m.FromUserId == FromUserId && m.ToUserId == model.ToUserId);
                      Request obj  = data ?? new();

                obj.FromUserId = FromUserId;
                obj.ToUserId = model.ToUserId;
                obj.CreatedDate = DateTime.Now;

                bool isPrivateUser = _dbcontext.Users.FirstOrDefault(m => m.UserId == model.ToUserId && m.IsDeleted == false)?.IsPrivate ??false;

                if(!isPrivateUser)
                {
                    obj.IsAccepted = true;
                }

                if(data == null)
                {
                   await _dbcontext.Requests.AddAsync(obj);
                }
                else
                {
                    obj.ModifiedDate = DateTime.Now;
                   
                    obj.IsDeleted = !data.IsDeleted;
                    if(obj.IsDeleted == true)
                    {
                        obj.IsAccepted = false;
                    }
                }
               await _dbcontext.SaveChangesAsync();

                if(isPrivateUser)
                {
                    await _helper.CreateNotification(new NotificationDTO()
                    {
                        FromUserId = FromUserId,
                        ToUserId = model.ToUserId,
                        NotificationType = NotificationType.FollowRequest,
                        NotificationTypeId = NotificationTypeId.RequestId,
                        Id = obj.RequestId,
                        IsDeleted = obj.IsDeleted,
                    });
                }
                else
                {
                    await _helper.CreateNotification(new NotificationDTO()
                    {
                        FromUserId = FromUserId,
                        ToUserId = model.ToUserId,
                        NotificationType = NotificationType.FollowRequestAccepted,
                        NotificationTypeId = NotificationTypeId.RequestId,
                        Id = obj.RequestId,
                        IsDeleted = obj.IsDeleted,
                    });
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
            IQueryable<UserDTO> Data = model.Model.FollowerOrFollowing switch
            {
                "Follower" => _dbcontext.Requests.Include(m => m.FromUser).Where(m => m.ToUserId == model.Model.UserId && m.IsAccepted != false && m.IsDeleted != true)
                .Select(m => new UserDTO
                {
                    UserId = m.FromUser.UserId,
                    UserName = m.FromUser.UserName,
                    Email = m.FromUser.Email,
                    Name = m.FromUser.Name,
                    Bio = m.FromUser.Bio,
                    Link = m.FromUser.Link,
                    Gender = m.FromUser.Gender ?? string.Empty,
                    ProfilePictureName = m.FromUser.ProfilePictureName,
                    ProfilePictureUrl = m.FromUser.ProfilePictureUrl,
                    ContactNumber = m.FromUser.ContactNumber,
                    IsPrivate = m.FromUser.IsPrivate,
                    IsVerified = m.FromUser.IsVerified,
                }),
                "Following" => _dbcontext.Requests.Include(m => m.ToUser).Where(m => m.FromUserId == model.Model.UserId && m.IsAccepted != false && m.IsDeleted != true)
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
                }),
                _ => throw new ArgumentException("Invalid FollowerOrFollowing value")
            };

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
        /// Retrieves a paginated list of users based on the provided search criteria asynchronously.
        /// </summary>
        /// <param name="model">The request details, including the search name, page number, and page size.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains a PaginationResponceModel of UserDTO which includes total records, page size, page number, required pages, and a list of user DTOs matching the search criteria.</returns>
        public async Task<PaginationResponceModel<UserDTO>> GetUserListByUserNameAsync(RequestDTO<UserIdRequestDTO> model)
        {
            long logInUserId = _helper.GetUserIdClaim();
            IQueryable<UserDTO> data = _dbcontext.Users
                 .Where(m => m.IsDeleted == false && m.UserId != logInUserId &&
                            (string.IsNullOrEmpty(model.SearchName) ||
                            (m.UserName ?? string.Empty).ToLower().Contains(model.SearchName.ToLower())))
                 .Select(user => new UserDTO
                 {
                     UserId = user.UserId,
                     UserName = user.UserName,
                     Email = user.Email,
                     Name = user.Name,
                     Bio = user.Bio,
                     Link = user.Link,
                     DateOfBirth = user.DateOfBirth.HasValue ? user.DateOfBirth.Value.ToString("yyyy-MM-dd") : string.Empty,
                     Gender = user.Gender ?? string.Empty,
                     ProfilePictureName = user.ProfilePictureName,
                     ProfilePictureUrl = user.ProfilePictureUrl,
                     ContactNumber = user.ContactNumber,
                     IsPrivate = user.IsPrivate,
                     IsVerified = user.IsVerified,
                     IsFollower =  _dbcontext.Requests.Any(r => r.FromUserId == user.UserId && r.ToUserId == logInUserId && r.IsAccepted == true && r.IsDeleted == false),
                     IsFollowing =  _dbcontext.Requests.Any(r => r.FromUserId == logInUserId && r.ToUserId == user.UserId && r.IsAccepted == true && r.IsDeleted == false),
                     IsRequest =  _dbcontext.Requests.Any(r => r.FromUserId == logInUserId && r.ToUserId == user.UserId && r.IsAccepted == false && r.IsDeleted == false),
                 });


            int totalRecords = await data.CountAsync();
            int requiredPages = (int)Math.Ceiling((decimal)totalRecords / model.PageSize);

            // Paginate the data
            List<UserDTO> records = await data
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
                .Include(m => m.FromUser) // Ensure FromUser is included for UserDTO
                .Where(m => m.ToUserId == model.Model.UserId && m.IsDeleted == false && m.IsAccepted == false)
                .OrderByDescending(r => r.CreatedDate)
                .Select(r => new RequestListResponseDTO
                {
                    RequestId = r.RequestId,
                    User = new UserDTO
                    {
                        UserId = r.FromUser.UserId,
                        UserName = r.FromUser.UserName,
                        Email = r.FromUser.Email,
                        Name = r.FromUser.Name,
                        Bio = r.FromUser.Bio,
                        Link = r.FromUser.Link,
                        Gender = r.FromUser.Gender ?? string.Empty,
                        ProfilePictureName = r.FromUser.ProfilePictureName ?? string.Empty,
                        ProfilePictureUrl = r.FromUser.ProfilePictureUrl ?? string.Empty,
                        ContactNumber = r.FromUser.ContactNumber ?? string.Empty,
                        IsPrivate = r.FromUser.IsPrivate,
                        IsVerified = r.FromUser.IsVerified,
                        IsFollower = _dbcontext.Requests.Any(req => req.FromUserId == r.FromUser.UserId && req.ToUserId == model.Model.UserId && req.IsAccepted == true && req.IsDeleted == false),
                        IsFollowing = _dbcontext.Requests.Any(req => req.FromUserId == model.Model.UserId && req.ToUserId == r.FromUser.UserId && req.IsAccepted == true && req.IsDeleted == false),
                        IsRequest = _dbcontext.Requests.Any(req => req.FromUserId == model.Model.UserId && req.ToUserId == r.FromUser.UserId && req.IsAccepted == false && req.IsDeleted == false),
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
            long logInUserId = _helper.GetUserIdClaim();
            User user = await _dbcontext.Users.FirstOrDefaultAsync(m => m.UserId == userId && !m.IsDeleted)
                        ?? throw new ValidationException(CustomErrorMessage.ExitsUser, CustomErrorCode.IsNotExits, new List<ValidationError>
                        {
                    new ValidationError
                    {
                        message = CustomErrorMessage.ExitsUser,
                        reference = "userId",
                        parameter = "userId",
                        errorCode = CustomErrorCode.IsNotExits
                    }
                        });

            UserDTO userDTO = new ()
            {
                UserId = user.UserId,
                UserName = user.UserName,
                Email = user.Email,
                Name = user.Name,
                Bio = user.Bio,
                Link = user.Link,
                DateOfBirth= user.DateOfBirth.HasValue ? user.DateOfBirth.Value.ToString("yyyy-MM-dd") : string.Empty,
                Gender = user.Gender ?? string.Empty,
                ProfilePictureName = user.ProfilePictureName,
                ProfilePictureUrl = user.ProfilePictureUrl,
                ContactNumber = user.ContactNumber,
                IsPrivate = user.IsPrivate,
                IsVerified = user.IsVerified,
                IsFollower = await _dbcontext.Requests.AnyAsync(r => r.FromUserId == user.UserId && r.ToUserId == logInUserId && r.IsAccepted == true && r.IsDeleted == false),
                IsFollowing = await _dbcontext.Requests.AnyAsync(r => r.FromUserId == logInUserId && r.ToUserId == user.UserId && r.IsAccepted == true && r.IsDeleted == false),
                IsRequest = await _dbcontext.Requests.AnyAsync(r => r.FromUserId == logInUserId && r.ToUserId == user.UserId && r.IsAccepted == false && r.IsDeleted == false),
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
            await _helper.CreateNotification(new NotificationDTO()
            {
                FromUserId = request.FromUserId,
                ToUserId = request.ToUserId,
                NotificationType = NotificationType.FollowRequestAccepted,
                NotificationTypeId = NotificationTypeId.RequestId,
                Id = requestId,
                IsDeleted = request.IsDeleted,
            });
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

        /// <summary>
        /// Retrieves a paginated list of suggested users asynchronously based on the current user's interactions and relationships.
        /// </summary>
        /// <param name="model">The pagination request details, including page number and page size.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains a PaginationResponceModel of UserDTO which includes total records, page size, page number, required pages, and a list of suggested user DTOs.</returns>
        public async Task<PaginationResponceModel<UserDTO>> GetSuggestionListAsync(PaginationRequestDTO model)
        {
            long userId = _helper.GetUserIdClaim();

            List<long> acceptedRequests = await _dbcontext.Requests
                .Where(r => r.FromUserId == userId && !r.IsDeleted && r.IsAccepted)
                .Select(r => r.ToUserId)
                .ToListAsync();

            List<long> pendingRequestsToCurrentUser = await _dbcontext.Requests
                .Where(r => r.ToUserId == userId && !r.IsDeleted && !r.IsAccepted)
                .Select(r => r.FromUserId)
                .ToListAsync();

            List<long> pendingRequestsByCurrentUser = await _dbcontext.Requests
                .Where(r => r.FromUserId == userId && !r.IsDeleted && !r.IsAccepted)
                .Select(r => r.ToUserId)
                .ToListAsync();

            List<long> suggestedUserIds = await _dbcontext.Users
                .Where(u => !u.IsDeleted
                            && u.UserId != userId
                            && !acceptedRequests.Contains(u.UserId)
                            && !pendingRequestsToCurrentUser.Contains(u.UserId)
                            && !pendingRequestsByCurrentUser.Contains(u.UserId))
                .Select(u => u.UserId)
                .ToListAsync();

            IQueryable<UserDTO> suggestedUsersQuery = _dbcontext.Users
                .Where(m => !m.IsDeleted && suggestedUserIds.Contains(m.UserId))
                .Select(user => new UserDTO
                {
                    UserId = user.UserId,
                    UserName = user.UserName,
                    Email = user.Email,
                    Name = user.Name,
                    Bio = user.Bio,
                    Link = user.Link,
                    DateOfBirth = user.DateOfBirth.HasValue ? user.DateOfBirth.Value.ToString("yyyy-MM-dd") : string.Empty,
                    Gender = user.Gender ?? string.Empty,
                    ProfilePictureName = user.ProfilePictureName,
                    ProfilePictureUrl = user.ProfilePictureUrl,
                    ContactNumber = user.ContactNumber,
                    IsPrivate = user.IsPrivate,
                    IsVerified = user.IsVerified,
                    IsFollower = _dbcontext.Requests.Any(r => r.FromUserId == user.UserId && r.ToUserId == userId && r.IsAccepted && !r.IsDeleted),
                    IsFollowing = _dbcontext.Requests.Any(r => r.FromUserId == userId && r.ToUserId == user.UserId && r.IsAccepted && !r.IsDeleted),
                    IsRequest = _dbcontext.Requests.Any(r => r.FromUserId == userId && r.ToUserId == user.UserId && !r.IsAccepted && !r.IsDeleted)
                });

            List<UserDTO> result = await suggestedUsersQuery
                .Skip((model.PageNumber - 1) * model.PageSize)
                .Take(model.PageSize)
                .ToListAsync();

            int totalRecords = await suggestedUsersQuery.CountAsync();
            int requiredPages = (int)Math.Ceiling((decimal)totalRecords / model.PageSize);

            return new PaginationResponceModel<UserDTO>
            {
                Totalrecord = totalRecords,
                PageSize = model.PageSize,
                PageNumber = model.PageNumber,
                RequirdPage = requiredPages,
                Record = result
            };
        }

        /// <summary>
        /// Inserts or updates a record in the searches table to indicate that a user has been searched for by the logged-in user.
        /// </summary>
        /// <param name="searchUserId">The ID of the user being searched for.</param>
        /// <returns>A task that represents the asynchronous operation. The task result indicates whether the operation was successful.</returns>
        public async Task<bool> UpsertSearchUserById(long searchUserId)
        {
            long logInUserId = _helper.GetUserIdClaim();

            Search? search =await _dbcontext.Searches.FirstOrDefaultAsync(m => m.SearchUserId == searchUserId && m.LoginUserId == logInUserId);
            Search obj = search ?? new Search();

            obj.LoginUserId = logInUserId;
            obj.SearchUserId = searchUserId;

            if (search != null)
            {
                obj.ModifiedDate = DateTime.Now;
                obj.IsDeleted = false;
                _dbcontext.Searches.Update(obj);
            }
            else
            {
                obj.CreatedDate = DateTime.Now;
                await _dbcontext.Searches.AddAsync(obj);
            }
            await _dbcontext.SaveChangesAsync();
            return true;
        }

        /// <summary>
        /// Marks a search record as deleted for the specified search ID and logged-in user ID.
        /// </summary>
        /// <param name="searchId">The ID of the search record to delete.</param>
        /// <returns>A task that represents the asynchronous operation. The task result indicates whether the deletion was successful.</returns>
        public async Task<bool> DeleteSearchUser(long searchId)
        {
            long userId = _helper.GetUserIdClaim();
            Search? search = await _dbcontext.Searches.FirstOrDefaultAsync(m => m.SearchId == searchId && m.LoginUserId == userId && m.IsDeleted == false);

            if (search != null)
            {
                search.IsDeleted = true;
                search.ModifiedDate = DateTime.UtcNow;

                _dbcontext.Searches.Update(search);
                await _dbcontext.SaveChangesAsync();
                return true;
            }
            return false;
        }

        /// <summary>
        /// Retrieves a paginated list of search records for the logged-in user.
        /// </summary>
        /// <param name="model">Pagination parameters including page number and page size.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains a pagination response model with the list of search records.</returns>
        public async Task<PaginationResponceModel<SearchDTO>> GetSearchUserList(PaginationRequestDTO model)
        {
            long userId = _helper.GetUserIdClaim();

            IQueryable<SearchDTO> data = _dbcontext.Searches.Where(m => m.LoginUserId == userId && m.IsDeleted == false)
                .Select(m => new SearchDTO
                {
                    SearchId = m.SearchId,
                    SearchUserId = m.SearchUserId,
                });

            List<SearchDTO> result = await data
               .Skip((model.PageNumber - 1) * model.PageSize)
               .Take(model.PageSize)
               .ToListAsync();

            int totalRecords = await data.CountAsync();
            int requiredPages = (int)Math.Ceiling((decimal)totalRecords / model.PageSize);

            return new PaginationResponceModel<SearchDTO>
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
