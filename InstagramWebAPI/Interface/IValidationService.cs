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
    }
}
