using DataAccess.CustomModel;
using InstagramWebAPI.DAL.Models;
using InstagramWebAPI.DTO;
using InstagramWebAPI.Helpers;
using InstagramWebAPI.Interface;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using static InstagramWebAPI.Utils.Enum;

namespace InstagramWebAPI.BLL
{
    public class NotificationService : INotificationService
    {
        public readonly ApplicationDbContext _dbcontext;
        private readonly Helper _helper;

        public NotificationService(ApplicationDbContext db, Helper helper)
        {
            _dbcontext = db;
            _helper = helper;
        }

        /// <summary>
        /// Retrieves a paginated list of notifications for the current user asynchronously.
        /// </summary>
        /// <param name="model">The pagination request details, including page number and page size.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains a PaginationResponceModel of NotificationResponseDTO which includes total records, page size, page number, required pages, and a list of notification response DTOs.</returns>
        public async Task<PaginationResponceModel<NotificationResponseDTO>> GetNotificationListById(PaginationRequestDTO model)
        {
            long userId = _helper.GetUserIdClaim();

            IQueryable<Notification> data = _dbcontext.Notifications
                .Where(m => m.ToUserId == userId && m.IsDeleted == false)
                .Include(n => n.FromUser)
            .Include(n => n.Story)
            .Include(n => n.Post).OrderByDescending(m=>m.CreatedDate); 

            List<Notification> paginatedNotifications = await data
                .Skip((model.PageNumber - 1) * model.PageSize)
                .Take(model.PageSize)
                .ToListAsync();

            int totalRecords = await data.CountAsync();
            int requiredPages = (int)Math.Ceiling((decimal)totalRecords / model.PageSize);

            List<NotificationResponseDTO> notificationResponses = paginatedNotifications.Select(n => new NotificationResponseDTO
            {
                NotificationId = n.NotificationId,
                UserId = n.FromUserId,
                UserName = n.FromUser?.UserName, // Safe navigation operator to avoid null reference
                ProfileName = n.FromUser?.ProfilePictureName, // Safe navigation operator to avoid null reference
                Message = GetMessageForNotification(n),
                StoryId = n.NotificationType == (int)NotificationType.StoryLiked ? n.StoryId ?? 0 : 0,
                PostId = (n.NotificationType == (int)NotificationType.PostLiked || n.NotificationType == (int)NotificationType.PostCommented) ? n.PostId ?? 0 : 0,
                Comment = n.NotificationType == (int)NotificationType.PostCommented ? _dbcontext.Comments.FirstOrDefault(m => m.CommentId == n.CommentId)?.CommentText : null,
                PhotoName = GetPhotoNameForNotification(n)
            }).ToList();

            return new PaginationResponceModel<NotificationResponseDTO>
            {
                Totalrecord = totalRecords,
                PageSize = model.PageSize,
                PageNumber = model.PageNumber,
                RequirdPage = requiredPages,
                Record = notificationResponses
            };
        }

        private string GetMessageForNotification(Notification n)
        {
            return n.NotificationType switch
            {
                (int)NotificationType.FollowRequest => "requested to follow you.",
                (int)NotificationType.FollowRequestAccepted => "Started following you",
                (int)NotificationType.FollowRequestDeleted => "has deleted your follow request.",
                (int)NotificationType.PostLiked => "liked your Photo.",
                (int)NotificationType.PostCommented => "commented on your post:",
                (int)NotificationType.StoryLiked => "liked your story.",
                _ => "You have a new notification."
            };
        }
        private string? GetPhotoNameForNotification(Notification n)
        {
            if (n.NotificationType == (int)NotificationType.PostLiked || n.NotificationType == (int)NotificationType.PostCommented)
            {
                return GetPostPhotoName(n.PostId);
            }
            if (n.NotificationType == (int)NotificationType.StoryLiked)
            {
                return GetStoryPhotoName(n.StoryId);
            }
            return null;
        }
        private string? GetPostPhotoName(long? postId)
        {
            return _dbcontext.PostMappings.FirstOrDefault(p => p.PostId == postId)?.MediaName;
        }
        private string? GetStoryPhotoName(long? storyId)
        {
            return _dbcontext.Stories.FirstOrDefault(s => s.StoryId == storyId)?.StoryName;
        }

        public async Task<bool> DeteleNotificationAsync(List<long> notificationId)
        {
            long userId = _helper.GetUserIdClaim();

            List<Notification> data = _dbcontext.Notifications.Where(m => notificationId.Contains(m.NotificationId) && m.IsDeleted != true && m.ToUserId == userId).ToList();
            data.ForEach(x =>
            {
                x.IsDeleted = true;
            });

            await _dbcontext.SaveChangesAsync();
            return true;
        }

    }
}
