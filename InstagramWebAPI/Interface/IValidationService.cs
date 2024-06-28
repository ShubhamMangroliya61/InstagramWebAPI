using DataAccess.CustomModel;
using InstagramWebAPI.Common;
using InstagramWebAPI.DTO;

namespace InstagramWebAPI.Interface
{
    public interface IValidationService
    {
        List<ValidationError> ValidateRegistration(UserDTO model);
        List<ValidationError> ValidateLogin(LoginRequestDTO model);
        List<ValidationError> ValidateForgotPassword(ForgotPasswordDTO model);
        List<ValidationError> ValidateForgotPasswordData(ForgotPasswordDTO model);
        List<ValidationError> ValidateProfileFile(UploadProfilePhotoDTO model);
        List<ValidationError> ValidateProfileData(UserDTO model);
        List<ValidationError> ValidateFollowRequest(FollowRequestDTO model);
        List<ValidationError> ValidateFollowerList(RequestDTO<FollowerListRequestDTO> model);
        List<ValidationError> ValidateRequestList(RequestDTO<FollowRequestDTO> model);
        List<ValidationError> ValidateGetUserById(long userid);
        List<ValidationError> ValidateRequestAccept(long requestId, string acceptType);
        List<ValidationError> ValidateCreatePost(CreatePostDTO model);
        List<ValidationError> ValidateResetPassword(ResetPasswordRequestDTO model);
        List<ValidationError> ValidatePostList(RequestDTO<PostListRequestDTO> model);
        List<ValidationError> ValidateDeletePostId(long postId);
        List<ValidationError> ValidateLikePost(long userId, long postId);
        bool IsUniqueUserName(string userName);
        bool IsUniqueEmail(UserDTO model);
        bool IsUniquePhoneNumber(UserDTO model);
    }
}
