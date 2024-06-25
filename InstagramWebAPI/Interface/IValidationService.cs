using InstagramWebAPI.Common;
using InstagramWebAPI.DTO;

namespace InstagramWebAPI.Interface
{
    public interface IValidationService
    {
        List<ValidationError> ValidateRegistration(RegistrationRequestDTO model);
        List<ValidationError> ValidateLogin(LoginRequestDTO model);
        List<ValidationError> ValidateResetPassword(ResetPasswordDTO model);
        List<ValidationError> ValidateResetPasswordData(ResetPasswordDTO model);
        List<ValidationError> ValidateProfileFile(UploadProfilePhotoDTO model);
        List<ValidationError> ValidateUserId(long userId);
        List<ValidationError> ValidateProfileData(UserDTO model);
        List<ValidationError> ValidateFollowRequest(FollowRequestDTO model);
    }
}
