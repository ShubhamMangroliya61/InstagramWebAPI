﻿namespace InstagramWebAPI.DTO
{
    public class LikePostDTO
    {
        public long UserId { get; set; }
        public long PostId { get; set; }
        public bool IsLike { get; set; }
    }
}
