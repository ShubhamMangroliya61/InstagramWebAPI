namespace InstagramWebAPI.DTO
{
    public class HighlightDTO
    {
        public long HighlightId { get; set; }
        public string? HighlightName { get; set; }
        public List<StoryHighLightList>? StoryHighLightLists { get; set; }
    }

    public class StoryHighLightList
    {
        public long StoryHighLightId { get; set; }
        public long StoryId { get; set; }
        public string? StoryUrl { get; set; }
        public string? StoryName { get; set; }
        public string? Caption { get; set; }
        public string? StoryType { get; set; }
        public DateTime CreatedDate { get; set; }
    }
}
