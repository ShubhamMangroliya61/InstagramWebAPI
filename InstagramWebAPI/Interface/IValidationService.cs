using DataAccess.CustomModel;
using InstagramWebAPI.Common;
using InstagramWebAPI.DTO;

namespace InstagramWebAPI.Interface
{
    public interface IValidationService
    {
        bool IsUniqueUserName(string userName, long userId);
        bool IsUniqueEmail(UserDTO model);
        bool IsUniquePhoneNumber(UserDTO model);
        List<ValidationError> ValidateRegistration(UserDTO model);
        List<ValidationError> ValidateLogin(LoginRequestDTO model);
        List<ValidationError> ValidateForgotPassword(ForgotPasswordDTO model);
        List<ValidationError> ValidateForgotPasswordData(ForgotPasswordDTO model);
        List<ValidationError> ValidateProfileFile(IFormFile ProfilePhoto, long userId);
        List<ValidationError> ValidateProfileData(UserDTO model);
        public List<ValidationError> ValidateFollowRequest(FollowRequestDTO model, long fromUserId);
        List<ValidationError> ValidateFollowerList(RequestDTO<FollowerListRequestDTO> model);
        List<ValidationError> ValidateRequestList(RequestDTO<FollowRequestDTO> model);
        List<ValidationError> ValidateGetUserById(long userId);
        List<ValidationError> ValidateRequestAccept(long requestId, string acceptType);
        List<ValidationError> ValidateCreatePost(CreatePostDTO model);
        List<ValidationError> ValidateResetPassword(ResetPasswordRequestDTO model);
        List<ValidationError> ValidatePostList(RequestDTO<PostListRequestDTO> model);
        List<ValidationError> ValidateDeletePostId(long postId);
        List<ValidationError> ValidateLikePost(long userId, long postId);
        List<ValidationError> ValidateCommentPost(CommentPostDTO model);
        List<ValidationError> ValidateCommentId(long commentId);
        List<ValidationError> ValidateStoryFile(AddStoryDTO model);
        List<ValidationError> ValidateStoryId(long storyId);
        List<ValidationError> ValidateGetStoryById(long userId, long storyId);
        List<ValidationError> ValidateMatualFrnd(RequestDTO<UserIdRequestDTO> model);
        List<ValidationError> ValidateUpsertHighLight(HighLightRequestDTO model);
        public List<ValidationError> ValidateHighLightId(long highLightId);
        List<ValidationError> ValidateAddStoryhighlight(long highLightId, long storyId, long userId);
        List<ValidationError> ValidateStoryHighLightId(long storyHighLightId);
        List<ValidationError> ValidatePostById(long postId, string postType);
        List<ValidationError> ValidateNotificationIds(List<long> notificationIds);
    }
}
