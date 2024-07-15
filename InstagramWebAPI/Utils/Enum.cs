namespace InstagramWebAPI.Utils
{
    public class Enum
    {
        public enum NotificationType
        {
            FollowRequest = 1,
            FollowRequestAccepted,
            FollowRequestDeleted,
            PostLiked,
            PostCommented,
            StoryLiked

        }
        public enum NotificationTypeId
        {
            LikeId,
            CommentId,
            RequestId,
            StoryId
        }
    }
}
