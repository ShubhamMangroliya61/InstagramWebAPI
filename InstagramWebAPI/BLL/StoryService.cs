﻿using DataAccess.CustomModel;
using InstagramWebAPI.Common;
using InstagramWebAPI.DAL.Models;
using InstagramWebAPI.DTO;
using InstagramWebAPI.Helpers;
using InstagramWebAPI.Interface;
using InstagramWebAPI.Utils;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using System.ComponentModel;
using System.Net;
using static InstagramWebAPI.Utils.Enum;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace InstagramWebAPI.BLL
{
    public class StoryService : IStoryService
    {
        public readonly ApplicationDbContext _dbcontext;
        private readonly Helper _helper;
        public StoryService(ApplicationDbContext db, Helper helper)
        {
            _dbcontext = db;
            _helper = helper;
        }

        /// <summary>
        /// Adds a story asynchronously based on the provided data in the <see cref="AddStoryDTO"/> model.
        /// </summary>
        /// <param name="model">The data transfer object containing the story file and associated data.</param>
        /// <returns>
        /// A task representing the asynchronous operation that returns a <see cref="StoryResponseListDTO"/> object representing the added story.
        /// </returns>
        public async Task<StoryResponseListDTO> AddStoryAsync(AddStoryDTO model)
        {
            long UserId = _helper.GetUserIdClaim();
            IFormFile file = model.Story ??
                  throw new ValidationException(CustomErrorMessage.NullStoryPhoto, CustomErrorCode.NullProfilePhoto, new List<ValidationError>
                   {
                       new ValidationError
                {
                    message = CustomErrorMessage.NullStoryPhoto,
                    reference = "ProfilePhoto",
                    parameter = "ProfilePhoto",
                    errorCode = CustomErrorCode.NullProfilePhoto
                }
                   });

            string userId = UserId.ToString();

            string path = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "content", "User", userId, "Story");

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

            int mediaTypeId = 0;

            if (file.ContentType.Contains("image"))
            {
                mediaTypeId = 1;
            }
            else if (file.ContentType.Contains("video"))
            {
                mediaTypeId = 2;
            }

            Story story = new()
            {
                UserId = UserId,
                StoryUrl = Path.Combine("content", "User", userId, "Story", fileName),
                StoryName = fileName,
                Caption = model.Caption,
                IsDeleted = false,
                CreatedDate = DateTime.Now,
                StoryTypeId = mediaTypeId,
            };

            await _dbcontext.Stories.AddAsync(story);
            await _dbcontext.SaveChangesAsync();
            

            StoryResponseListDTO storyResponseDTO = await GetStoryById(UserId, story.StoryId);
            return storyResponseDTO;
        }

        /// <summary>
        /// Soft-deletes a story asynchronously based on the provided story ID and current user ID.
        /// </summary>
        /// <param name="storyId">The unique identifier of the story to delete.</param>
        /// <returns>
        /// A task representing the asynchronous operation that returns a boolean indicating whether the story was successfully soft-deleted.
        /// Returns true if the story was found and soft-deleted.
        /// Returns false if no story with the specified ID was found, the story belongs to a
        /// </returns>
        public async Task<bool> DeteleStoryAsync(long storyId)
        {
            long userId = _helper.GetUserIdClaim();
            Story? story = await _dbcontext.Stories.FirstOrDefaultAsync(m => m.StoryId == storyId && m.UserId == userId && m.IsDeleted == false);

            if (story != null)
            {
                story.IsDeleted = true;
                story.ModifiedDate = DateTime.UtcNow;

                _dbcontext.Stories.Update(story);
                await _dbcontext.SaveChangesAsync();
                return true;
            }

            return false;
        }

        /// <summary>
        /// Retrieves a specific story by its ID asynchronously.
        /// </summary>
        /// <param name="userId">The ID of the user requesting the story.</param>
        /// <param name="storyId">The ID of the story to retrieve.</param>
        /// <returns>
        /// A task representing the asynchronous operation that returns a <see cref="StoryResponseListDTO"/> object containing the details of the retrieved story.
        /// </returns>
        public async Task<StoryResponseListDTO> GetStoryById(long userId, long storyId)
        {
            Story story = await _dbcontext.Stories
                .Include(m => m.User)
                .FirstOrDefaultAsync(m => m.StoryId == storyId && m.IsDeleted == false) ??

                throw new ValidationException(CustomErrorMessage.NullStoryPhoto, CustomErrorCode.NullProfilePhoto, new List<ValidationError>
                   {
                       new ValidationError
                        {
                            message = CustomErrorMessage.NullStoryPhoto,
                            reference = "story",
                            parameter = "story",
                            errorCode = CustomErrorCode.NullProfilePhoto
                        }
                   });

            List<StoryViewByUserList> storyViewList = _dbcontext.StoryViews
                           .Where(view => view.StoryId == storyId)
                           .Join(_dbcontext.Users,
                                 view => view.StoryViewUserId,
                                 user => user.UserId,
                                 (view, user) => new StoryViewByUserList
                                 {
                                     UserId = user.UserId,
                                     UserName = user.UserName,
                                     ProfilePictureName = user.ProfilePictureName,
                                     IsLike = view.IsLike
                                 })
                           .ToList();

            return new StoryResponseListDTO
            {
                UserId = story.UserId,
                UserName = story.User.UserName,
                ProfilePictureName = story.User.ProfilePictureName,
                Stories = new List<StoryList>
                {
                    new StoryList
                    {
                        StoryId = story.StoryId,
                        StoryUrl = story.StoryUrl,
                        StoryName = story.StoryName,
                        Caption = story.Caption,
                        StoryType = story.StoryTypeId == 1 ? "Image" : "Video",
                        IsSeen =  _dbcontext.StoryViews.Any(m => m.StoryViewUserId == userId && m.StoryId == storyId),
                        CreatedDate = story.CreatedDate,
                        StoryViewList = storyViewList
                    }
                }
            };
        }

        /// <summary>
        /// Retrieves a paginated list of stories for the current user asynchronously.
        /// </summary>
        /// <param name="model">The pagination request details, including page number and page size.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains a PaginationResponceModel of StoryResponseListDTO which includes total records, page size, page number, required pages, and a list of story response DTOs.</returns>
        public async Task<PaginationResponceModel<StoryResponseListDTO>> GetStoryListByIdAsync(PaginationRequestDTO model)
        {
            long userId = _helper.GetUserIdClaim();
            DateTime cutoffDate = DateTime.UtcNow.AddDays(-1);

            // Querying stories for the user
            IQueryable<Story> storiesList =_dbcontext.Stories.Include(m=>m.User)
                .Include(s => s.StoryViews).ThenInclude(s => s.StoryViewUser)
                .Where(s => s.UserId == userId && !s.IsDeleted && s.CreatedDate >= cutoffDate)
                .OrderByDescending(s => s.CreatedDate);
              
            int totalRecords =await storiesList.CountAsync();

            List<Story> stories =await storiesList.Skip((model.PageNumber - 1) * model.PageSize)
                .Take(model.PageSize)
                .ToListAsync();

            List<StoryList> storyListData = stories.Select(story => new StoryList
            {
                StoryId = story.StoryId,
                StoryUrl = story.StoryUrl,
                StoryName = story.StoryName,
                Caption = story.Caption,
                StoryType = story.StoryTypeId == 1 ? "Image" : "Video",
                CreatedDate = story.CreatedDate,
                IsSeen = story.StoryViews.Any(view => view.StoryViewUserId == userId),
                StoryViewList = story.StoryViews.Select(view => new StoryViewByUserList
                {
                    UserId = view.StoryViewUserId,
                    UserName = view.StoryViewUser?.UserName,
                    ProfilePictureName = view.StoryViewUser?.ProfilePictureName,
                    IsLike = view.IsLike
                }).ToList()
            }).ToList();

            StoryResponseListDTO data = new ()
            {
                UserId = userId,
                UserName = stories.FirstOrDefault()?.User.UserName,
                ProfilePictureName = stories.FirstOrDefault()?.User.ProfilePictureName,
                Stories = storyListData
            };

            var requiredPages = (int)Math.Ceiling((decimal)totalRecords / model.PageSize);

            var paginationResponse = new PaginationResponceModel<StoryResponseListDTO>
            {
                Totalrecord = totalRecords,
                PageSize = model.PageSize,
                PageNumber = model.PageNumber,
                RequirdPage = requiredPages,
                Record = new List<StoryResponseListDTO> { data }
            };

            return paginationResponse;
        }

        /// <summary>
        /// Retrieves a paginated list of stories for a specified user ID asynchronously.
        /// </summary>
        /// <param name="model">The request model containing the user ID and pagination details.</param>
        /// <returns>
        /// A task representing the asynchronous operation that returns a <see cref="PaginationResponceModel{T}"/> of <see cref="StoryResponseListDTO"/>.
        /// The response contains a paginated list of stories with details such as user ID, username, profile picture, and stories associated with the user.
        /// </returns>
        public async Task<PaginationResponceModel<StoryResponseListDTO>> GetStoryListByUserIdAsync(RequestDTO<UserIdRequestDTO> model)
        {
            DateTime cutoffDate = DateTime.UtcNow.AddDays(-1);

            IQueryable<StoryResponseListDTO> data = _dbcontext.Stories
                        .Include(s => s.User) // Include the User related to the story
                        .Where(s => !s.IsDeleted && s.CreatedDate >= cutoffDate)
                        .Join(_dbcontext.Requests,
                              story => story.UserId,
                              request => request.ToUserId,
                              (story, request) => new { Story = story, Request = request })
                        .Where(joined => joined.Request.FromUserId == model.Model.UserId && joined.Request.IsAccepted)
                        .GroupBy(joined => new { joined.Story.UserId, joined.Story.User.UserName, joined.Story.User.ProfilePictureName })
                        .Select(g => new StoryResponseListDTO
                        {
                            UserId = g.Key.UserId,
                            UserName = g.Key.UserName,
                            ProfilePictureName = g.Key.ProfilePictureName,
                            Stories = g.Select(s => new StoryList
                            {
                                StoryId = s.Story.StoryId,
                                StoryUrl = s.Story.StoryUrl,
                                StoryName = s.Story.StoryName,
                                Caption = s.Story.Caption,
                                StoryType = s.Story.StoryTypeId == 1 ? "Image" : "Video",
                                CreatedDate = s.Story.CreatedDate,
                                IsSeen = _dbcontext.StoryViews.Any(m => m.StoryViewUserId == model.Model.UserId && m.StoryId == s.Story.StoryId)
                            }).ToList()
                        });

            int totalRecords = await data.CountAsync();
            int requiredPages = (int)Math.Ceiling((decimal)totalRecords / model.PageSize);

            // Paginate the data
            List<StoryResponseListDTO> records = await data
                .Skip((model.PageNumber - 1) * model.PageSize)
                .Take(model.PageSize)
                .ToListAsync();

            return new PaginationResponceModel<StoryResponseListDTO>
            {
                Totalrecord = totalRecords,
                PageSize = model.PageSize,
                PageNumber = model.PageNumber,
                RequirdPage = requiredPages,
                Record = records,
            };
        }

        /// <summary>
        /// Records that a specific story has been viewed by the authenticated user asynchronously.
        /// </summary>
        /// <param name="storyId">The ID of the story that has been viewed.</param>
        /// <returns>
        /// A task representing the asynchronous operation that indicates whether the operation to record the story view was successful (<c>true</c>) or not (<c>false</c>).
        /// </returns>
        public async Task<bool> StorySeenByUserIdAsync(long storyId)
        {
            long userId = _helper.GetUserIdClaim();
            StoryView? data=await _dbcontext.StoryViews.FirstOrDefaultAsync(m=>m.StoryId == storyId && m.StoryViewUserId == userId);
            StoryView obj = data ?? new();

            obj.StoryId = storyId;
            obj.StoryViewUserId = userId;
            obj.IsLike = false;

            if(data != null)
            {
                _dbcontext.StoryViews.Update(obj);
            }
            else
            {
                obj.CreatedDate = DateTime.Now;
                await _dbcontext.StoryViews.AddAsync(obj);
            }
            await _dbcontext.SaveChangesAsync();
            return true;
        }

        /// <summary>
        /// Likes or unlikes a specific story asynchronously for the authenticated user.
        /// </summary>
        /// <param name="storyId">The ID of the story to like or unlike.</param>
        /// <param name="isLike">Boolean indicating whether to like (<c>true</c>) or unlike (<c>false</c>) the story.</param>
        /// <returns>
        /// A task representing the asynchronous operation that indicates whether the operation to like/unlike the story was successful (<c>true</c>) or not (<c>false</c>).
        /// </returns>
        public async Task<bool> LikeStoryAsync(long storyId, bool isLike)
        {
            long userId = _helper.GetUserIdClaim();
            StoryView? story = await _dbcontext.StoryViews.FirstOrDefaultAsync(m => m.StoryId == storyId && m.StoryViewUserId == userId);
            StoryView obj = story ?? new();

            obj.StoryId = storyId;
            obj.StoryViewUserId = userId;
            obj.IsLike = isLike;

            if (story != null)
            {
                _dbcontext.StoryViews.Update(story);
            }
            else
            {
                obj.CreatedDate = DateTime.Now;
                await _dbcontext.StoryViews.AddAsync(obj);
            }
            await _dbcontext.SaveChangesAsync();

            long toUserId = _dbcontext.Stories.FirstOrDefaultAsync(m => m.StoryId == storyId && m.IsDeleted == false).Result?.UserId ?? 0;
            if (toUserId != userId)
            {
                await _helper.CreateNotification(new NotificationDTO()
                {
                    FromUserId = userId,
                    ToUserId = toUserId,
                    NotificationType = NotificationType.StoryLiked,
                    NotificationTypeId = NotificationTypeId.StoryId,
                    Id = obj.StoryId,
                    IsDeleted = (bool)obj.IsLike,
                });
            }
            return true;
        }

        /// <summary>
        /// Inserts or updates a highlight asynchronously based on the provided highlight request details.
        /// </summary>
        /// <param name="model">The highlight request details, including highlight ID and highlight name.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains the upserted HighlightDTO which includes the highlight ID and highlight name.</returns>
        public async Task<HighlightDTO> UpsertHighlightAsync(HighLightRequestDTO model)
        {
            long userId = _helper.GetUserIdClaim();
            Highlight highlight = await _dbcontext.Highlights.FirstOrDefaultAsync(m => m.HighlightsId == model.HighlightId && m.UserId == userId && m.IsDeleted == false) ?? new();

            highlight.HighlightsName = model.HighlightName ?? string.Empty;
            highlight.UserId = userId;

            if (model.HighlightId > 0)
            {
                highlight.ModifiedDate = DateTime.Now;
                _dbcontext.Highlights.Update(highlight);
                await _dbcontext.SaveChangesAsync();
            }
            else
            {
                highlight.CreatedDate = DateTime.Now;
                await _dbcontext.Highlights.AddAsync(highlight);
                await _dbcontext.SaveChangesAsync();
            }

            return new HighlightDTO()
            {
                HighlightId = highlight.HighlightsId,
                HighlightName = highlight.HighlightsName,
            };
        }

        /// <summary>
        /// Deletes a highlight asynchronously by marking it as deleted based on the provided highlight ID.
        /// </summary>
        /// <param name="highLightId">The ID of the highlight to delete.</param>
        /// <returns>A task that represents the asynchronous operation. The task result is a boolean indicating whether the highlight was successfully marked as deleted.</returns>
        public async Task<bool> DeteleHighLightAsync(long highLightId)
        {
            long userId = _helper.GetUserIdClaim();
            Highlight? highlight = await _dbcontext.Highlights.FirstOrDefaultAsync(m => m.HighlightsId == highLightId && m.UserId == userId && m.IsDeleted == false);

            if (highlight != null)
            {
                highlight.IsDeleted = true;
                highlight.ModifiedDate = DateTime.UtcNow;

                _dbcontext.Highlights.Update(highlight);
                await _dbcontext.SaveChangesAsync();
                return true;
            }
            return false;
        }

        /// <summary>
        /// Associates a story with a highlight asynchronously based on the provided highlight ID and story ID.
        /// </summary>
        /// <param name="highLightId">The ID of the highlight to which the story will be associated.</param>
        /// <param name="storyId">The ID of the story to be associated with the highlight.</param>
        /// <returns>A task that represents the asynchronous operation. The task result is a boolean indicating whether the story was successfully associated with the highlight.</returns>
        public async Task<bool> AddStoryHighLightAsync(long highLightId, long storyId)
        {
            StoryHighlight data = new()
            {
                StoryId = storyId,
                HighlightsId = highLightId,
                CreatedDate = DateTime.Now,
            };
            await _dbcontext.StoryHighlights.AddAsync(data);
            await _dbcontext.SaveChangesAsync();
            return true;
        }

        /// <summary>
        /// Deletes a story-highlight association asynchronously by marking it as deleted based on the provided story highlight ID.
        /// </summary>
        /// <param name="storyHighLightId">The ID of the story-highlight association to delete.</param>
        /// <returns>A task that represents the asynchronous operation. The task result is a boolean indicating whether the story-highlight association was successfully marked as deleted.</returns>
        public async Task<bool> DeleteStoryHighLightAsync(long storyHighLightId)
        {
            StoryHighlight? highlight = await _dbcontext.StoryHighlights.FirstOrDefaultAsync(m => m.StoryHighlightId == storyHighLightId && m.IsDeleted == false);

            if (highlight != null)
            {
                highlight.IsDeleted = true;
                highlight.ModifiedDate = DateTime.UtcNow;

                _dbcontext.StoryHighlights.Update(highlight);
                await _dbcontext.SaveChangesAsync();
                return true;
            }
            return false;
        }

        /// <summary>
        /// Retrieves a paginated list of highlights for a specified user asynchronously.
        /// </summary>
        /// <param name="model">The request details, including the user ID, page number, and page size.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains a PaginationResponceModel of HighlightDTO which includes total records, page size, page number, required pages, and a list of highlight DTOs.</returns>
        public async Task<PaginationResponceModel<HighlightDTO>> GetHighLightListByUserId(RequestDTO<UserIdRequestDTO> model)
        {
            IQueryable<HighlightDTO> data = _dbcontext.Highlights
                        .Include(m => m.StoryHighlights).ThenInclude(m => m.Story)
                        .Where(m => m.UserId == model.Model.UserId)
                        .Select(m => new HighlightDTO
                        {
                            HighlightId = m.HighlightsId,
                            HighlightName = m.HighlightsName,
                            StoryHighLightLists = m.StoryHighlights.Select(s => new StoryHighLightList
                            {
                                StoryHighLightId = s.StoryHighlightId,
                                StoryId = s.StoryId,
                                StoryUrl = s.Story.StoryUrl,
                                StoryName = s.Story.StoryName,
                                Caption = s.Story.Caption,
                                StoryType = s.Story.StoryTypeId == 1 ? "Image" : "Video",
                                CreatedDate = s.Story.CreatedDate
                            }).ToList()
                        });

            int totalRecords = await data.CountAsync();
            int requiredPages = (int)Math.Ceiling((decimal)totalRecords / model.PageSize);

            // Paginate the data
            List<HighlightDTO> records = await data
                .Skip((model.PageNumber - 1) * model.PageSize)
                .Take(model.PageSize)
                .ToListAsync();

            return new PaginationResponceModel<HighlightDTO>
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
