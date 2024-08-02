namespace InstagramWebAPI.DTO
{
    public class ChatDTO
    {
        public long ChatId { get; set; }
        public long ToUserId { get; set; }
        public string? ToUserName { get; set; }
        public string? ProfileName { get; set; }
        public string? LastMessage { get; set; }
        public int Unread { get; set; }
        public bool IsOnline { get; set; }
        public DateTime? CreatedDate { get; set; }
    }
}
