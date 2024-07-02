namespace InstagramWebAPI.DTO
{
    public class CreatePostDTO
    {
        public long PostId { get; set; }
        public string? Caption { get; set; }
        public string? Location { get; set; }
        public string? PostType { get; set; }
        public List<IFormFile>? File { get; set; }
    }
}
