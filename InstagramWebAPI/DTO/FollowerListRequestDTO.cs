namespace InstagramWebAPI.DTO
{
    public class FollowerListRequestDTO
    {
        public long UserId { get; set; }
        public string? FollowerOrFollowing { get; set; }
       
    }
}
