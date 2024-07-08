using static InstagramWebAPI.Utils.Enum;

namespace InstagramWebAPI.DTO
{
    public class NotificationDTO
    {
        public long NotificationId { get; set; }
        public long FromUserId { get; set; }
        public long ToUserId { get; set; }
        public NotificationType NotificationType { get; set; } 
        public long Id { get; set; }
        public NotificationTypeId NotificationTypeId { get; set; }
        public bool IsDeleted { get; set; }
      
    }
}
