using InstagramWebAPI.DAL.Models;
using InstagramWebAPI.DTO;

namespace InstagramWebAPI.Interface
{
    public interface IAuthService
    {
        Task<bool> IsUniqueUserName(string userName);
        Task<UserDTO> UserRegisterAsync(RegistrationRequestDTO model);
        Task<LoginResponseDTO> UserLoginAsync(LoginRequestDTO model);
        Task<User> GetUser(ResetPasswordDTO model);
        Task<bool> ResetPasswordAsync(ResetPasswordDTO model);
    }
}
