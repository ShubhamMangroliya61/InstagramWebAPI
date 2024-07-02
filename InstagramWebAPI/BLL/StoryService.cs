using DataAccess.CustomModel;
using InstagramWebAPI.Common;
using InstagramWebAPI.DAL.Models;
using InstagramWebAPI.DTO;
using InstagramWebAPI.Helpers;
using InstagramWebAPI.Interface;
using InstagramWebAPI.Utils;
using Microsoft.EntityFrameworkCore;
using System.Net;

namespace InstagramWebAPI.BLL
{
    public class StoryService : IStoryService
    {
        public readonly ApplicationDbContext _dbcontext;
        private readonly Helper _helper;
        public StoryService(ApplicationDbContext db,Helper helper)
        {
            _dbcontext = db;
            _helper = helper;
        }

        public async Task<StoryResponseListDTO> AddStoryAsync(AddStoryDTO model)
        {
            long UserId =_helper.GetUserIdClaim();
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

            StoryResponseListDTO storyResponseDTO =await GetStoryById(UserId, story.StoryId);
            return storyResponseDTO;
        }

        public async Task<bool> DeteleStoryAsync(long storyId)
        {
            long userId = _helper.GetUserIdClaim();
            Story? story = await _dbcontext.Stories.FirstOrDefaultAsync(m => m.StoryId == storyId && m.UserId==userId && m.IsDeleted == false);

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
                            reference = "ProfilePhoto",
                            parameter = "ProfilePhoto",
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
                                     IsLike=view.IsLike
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

        public async Task<bool> StorySeenByUserIdAsync(long storyId)
        {
            long userId = _helper.GetUserIdClaim();
            StoryView storyView = new()
            {
                StoryId = storyId,
                StoryViewUserId = userId,
                CreatedDate = DateTime.Now,
                IsLike = false,
            };
            await _dbcontext.StoryViews.AddAsync(storyView);
            await _dbcontext.SaveChangesAsync();
            return true;
        }

        public async Task<bool> LikeStoryAsync(long storyId,bool isLike)
        {
            long userId = _helper.GetUserIdClaim();
            StoryView? story =await _dbcontext.StoryViews.FirstOrDefaultAsync(m=>m.StoryId == storyId && m.StoryViewUserId == userId);
            
            if(story == null)
            {
                StoryView storyView = new()
                {
                    StoryId = storyId,
                    StoryViewUserId = userId,
                    CreatedDate = DateTime.Now,
                    IsLike = isLike,
                };
                await _dbcontext.StoryViews.AddAsync(storyView);
                await _dbcontext.SaveChangesAsync();
                return true;
            }
            else
            {
                story.IsLike=isLike;
                _dbcontext.StoryViews.Update(story);
                await _dbcontext.SaveChangesAsync();
                return true;
            }
        }
    }
}
