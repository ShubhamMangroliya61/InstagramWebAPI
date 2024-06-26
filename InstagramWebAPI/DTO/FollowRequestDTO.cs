namespace InstagramWebAPI.DTO
{
    public class FollowRequestDTO
    {
        public long UserId { get; set; }
        public long FromUserId { get; set; }
        public long ToUserId { get; set; }
    }
}
