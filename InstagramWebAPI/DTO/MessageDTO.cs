namespace InstagramWebAPI.DTO
{
    public class MessageDTO
    {
        public long MessagesId { get; set; }
        public long ChatId { get; set; }
        public long FromUserId { get; set; }
        public long ToUserId { get; set; }
        public string? MessageText { get; set; }
        public bool IsSeen { get; set; }
        public DateTime? CreatedDate { get; set; }

    }
}
