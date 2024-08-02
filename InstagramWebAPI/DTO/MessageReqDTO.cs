namespace InstagramWebAPI.DTO
{
    public class MessageReqDTO
    {
        public long ToUserId { get; set; }
        public long ChatId { get; set; }
        public string? Messages { get; set; }
        public bool IsDeliverd { get; set; }
        public long FromUserId { get; set; }
    }
}
