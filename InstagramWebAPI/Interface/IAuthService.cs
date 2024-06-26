using InstagramWebAPI.DAL.Models;
using InstagramWebAPI.DTO;

namespace InstagramWebAPI.Interface
{
    public interface IAuthService
    {
        Task<bool> IsUniqueUserNameEmailPhoneNumber(UserDTO model);
        Task<User> UpSertUserAsync(UserDTO model);
        Task<LoginResponseDTO> UserLoginAsync(LoginRequestDTO model);
        Task<User> GetUser(ForgotPasswordDTO model);
        Task<bool> ForgotPasswordAsync(ForgotPasswordDTO model);
        Task<bool> IsUniqueUserName(string userName);
        Task<bool> ResetPasswordAsync(ResetPasswordRequestDTO model);
    }
}
