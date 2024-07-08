using System.Net.Mail;
using System.Net;
using InstagramWebAPI.DAL.Models;
using System.Security.Claims;
using InstagramWebAPI.DTO;
using InstagramWebAPI.Utils;
using static InstagramWebAPI.Utils.Enum;
using Microsoft.EntityFrameworkCore;

namespace InstagramWebAPI.Helpers
{
    public class Helper
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ApplicationDbContext _dbcontext;
        public Helper(IHttpContextAccessor httpContextAccessor, ApplicationDbContext db)
        {
            _httpContextAccessor = httpContextAccessor;
            _dbcontext = db;
        }
        public async Task<bool> EmailSender(string email, string subject, string htmlMessage)
        {
            try
            {
                var mail = "tatva.dotnet.shubhammangroliya@outlook.com";
                var password = "snwwkdrbhcdxifyc";

                var client = new SmtpClient("smtp.office365.com", 587)
                {
                    EnableSsl = true,
                    Credentials = new NetworkCredential(mail, password)
                };

                var mailMessage = new MailMessage
                {
                    From = new MailAddress(mail),
                    Subject = subject,
                    Body = htmlMessage,
                    IsBodyHtml = true
                };
                mailMessage.To.Add(email);

                await client.SendMailAsync(mailMessage);

                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }
        public long GetUserIdClaim()
        {
            var userIdClaim = _httpContextAccessor?.HttpContext?.User.FindFirst("UserId");

            if (userIdClaim != null && long.TryParse(userIdClaim.Value, out long userId))
            {
                return userId;
            }
            return 0;
        }

        public UserDTO UserMapper(User user)
        {
            UserDTO userDTO = new()
            {
                UserId = user.UserId,
                UserName = user.UserName,
                Email = user.Email,
                Name = user.Name,
                Bio = user.Bio,
                Link = user.Link,
                Gender = user.Gender ?? string.Empty,
                DateOfBirth = user.DateOfBirth.HasValue ? user.DateOfBirth.Value.ToString("yyyy-MM-dd") : string.Empty,
                ProfilePictureName = user.ProfilePictureName ?? string.Empty,
                ProfilePictureUrl = user.ProfilePictureUrl ?? string.Empty,
                ContactNumber = user.ContactNumber ?? string.Empty,
                IsPrivate = user.IsPrivate,
                IsVerified = user.IsVerified,
                // Map other properties as needed
            };
            return userDTO;
        }

        public async Task CreateNotification(NotificationDTO model)
        {
            IQueryable<Notification> data = _dbcontext.Notifications.Where(m => m.FromUserId == model.FromUserId && m.ToUserId == model.ToUserId);

            Notification? obj = model.NotificationTypeId switch
            {
                NotificationTypeId.PostId => await  data.FirstOrDefaultAsync(m => m.PostId == model.Id),
                NotificationTypeId.LikeId =>await data.FirstOrDefaultAsync(m => m.LikeId == model.Id),
                NotificationTypeId.CommentId =>await data.FirstOrDefaultAsync(m => m.CommentId == model.Id),
                NotificationTypeId.RequestId =>await data.FirstOrDefaultAsync(m => m.RequestId == model.Id),
                NotificationTypeId.StoryId =>await data.FirstOrDefaultAsync(m => m.StoryId == model.Id),
                _ => throw new ArgumentOutOfRangeException(nameof(model.NotificationId), "Unknown NotificationId type"),
            };

            Notification notification = obj ?? new Notification();

            notification.FromUserId = model.FromUserId;
            notification.ToUserId = model.ToUserId;
            notification.NotificationType = (int)model.NotificationType;
            notification.CreatedDate = DateTime.Now;
            notification.IsDeleted= model.IsDeleted;
            if (obj == null)
            {
                switch (model.NotificationTypeId)
                {
                    case NotificationTypeId.PostId:
                        notification.PostId = model.Id;
                        break;
                    case NotificationTypeId.LikeId:
                        notification.LikeId = model.Id;
                        break;
                    case NotificationTypeId.CommentId:
                        notification.CommentId = model.Id;
                        break;
                    case NotificationTypeId.RequestId:
                        notification.RequestId = model.Id;
                        break;
                    case NotificationTypeId.StoryId:
                        notification.StoryId = model.Id;
                        break;
                }
                _dbcontext.Notifications.Add(notification);
            }
            await _dbcontext.SaveChangesAsync();
        }
    }
}
