namespace InstagramWebAPI.DTO
{
    
    public class CollectionDTO
    {
        public long CollectionId { get; set; }
        public string? CollectionName { get; set; }
        public List<PostCollectionList>? PostCollectionList { get; set; }
    }

    public class PostCollectionList
    {
        public long PostCollectionId { get; set; }
        public long PostId { get; set; }
        public DateTime CreatedDate { get; set; }
    }
}
