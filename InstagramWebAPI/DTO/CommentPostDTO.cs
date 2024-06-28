namespace InstagramWebAPI.DTO
{
    public class CommentPostDTO
    {
        public long UserId { get; set; }
        public long PostId { get; set; }
        public string? CommentText { get; set; }
    }
}
