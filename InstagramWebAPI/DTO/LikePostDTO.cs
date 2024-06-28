namespace InstagramWebAPI.DTO
{
    public class LikePostDTO
    {
        public long userId { get; set; }
        public long postId { get; set; }
        public bool isLike { get; set; }
    }
}
