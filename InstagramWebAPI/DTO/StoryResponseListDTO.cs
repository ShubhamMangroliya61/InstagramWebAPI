namespace InstagramWebAPI.DTO
{
    public class StoryResponseListDTO
    {
        
        public long UserId { get; set; }
        public string? UserName { get; set; }
        public string? ProfilePictureName { get; set; }
        public List<StoryList>? Stories { get; set; }
    }

    public class StoryList
    {
        public long StoryId { get; set; }
        public string? StoryUrl { get; set; }
        public string? StoryName { get; set; }
        public string? Caption { get; set; }
        public string? StoryType { get; set; }
        public DateTime CreatedDate { get; set; }
        public bool IsSeen { get; set; }
        public List<StoryViewByUserList>? StoryViewList { get; set; }
    }

    public class StoryViewByUserList
    {
        public long UserId { get; set; }
        public string? UserName { get; set; }
        public string? ProfilePictureName { get; set; }
        public bool? IsLike { get; set; }
    }
}
