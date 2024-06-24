using InstagramWebAPI.DAL.Models;
using InstagramWebAPI.DTO;

namespace InstagramWebAPI.Interface
{
    public interface IUserService
    {
        Task<bool> IsUniqueUserName(string userName);
        Task<User> UserRegisterAsync(RegistrationRequestDTO model);
        Task<LoginResponseDTO> UserLoginAsync(LoginRequestDTO model);
        Task<User> GetUser(ResetPasswordDTO model);
        Task<bool> ForgotPasswordData(ResetPasswordDTO model);
    }
}
