namespace InstagramWebAPI.DTO
{
    public class PostResponseDTO
    {
        public long PostId { get; set; }
        public long UserId { get; set; }
        public string? Caption { get; set; }
        public string? Location { get; set; }
        public string? PostType { get; set; }
        public List<Media>? Medias { get; set; }
    }

    public class Media
    {
        public long PostMappingId { get; set; }
        public string? MediaType { get; set; }
        public string? MediaURL { get; set;}
        public string? MediaName { get; set; }

    }
    
    public class PostLike
    {
        public long LikeId { get; set; }
        public long UserId { get; set; }
        public string? Avtar { get; set; }
        public string? UserName { get; set; }
    }

    public class PostComment
    {
        public long CommentId { get; set; }
        public long UserId { get; set; }
        public string? CommentText { get; set;}
        public string? Avtar { get; set;}
        public string? UserName { get; set;}
    }
}
