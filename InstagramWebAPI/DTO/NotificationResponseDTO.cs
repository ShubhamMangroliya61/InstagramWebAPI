namespace InstagramWebAPI.DTO
{
    public class NotificationResponseDTO
    {
        public long NotificationId { get; set; }
        public long UserId { get; set; }
        public string? UserName { get; set; }
        public string? ProfileName { get; set; }
        public string? Message { get; set; }
        public long? StoryId { get; set; }
        public long? PostId { get; set; }
        public string? Comment { get; set;}
        public string? PhotoName { get; set; }
    }
}
