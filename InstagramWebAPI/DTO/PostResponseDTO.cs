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
}
