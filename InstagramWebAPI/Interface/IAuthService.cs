using InstagramWebAPI.DAL.Models;
using InstagramWebAPI.DTO;

namespace InstagramWebAPI.Interface
{
    public interface IAuthService
    {
        Task<bool> IsUniqueUserNameEmailPhoneNumber(RegistrationRequestDTO model);
        Task<UserDTO> UserRegisterAsync(RegistrationRequestDTO model);
        Task<LoginResponseDTO> UserLoginAsync(LoginRequestDTO model);
        Task<User> GetUser(ResetPasswordDTO model);
        Task<bool> ResetPasswordAsync(ResetPasswordDTO model);
        Task<bool> IsUniqueUserName(string userName);
    }
}
