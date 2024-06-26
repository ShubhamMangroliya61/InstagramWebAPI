using InstagramWebAPI.DAL.Models;
using InstagramWebAPI.DTO;
using InstagramWebAPI.Interface;
using InstagramWebAPI.Utils;
using Microsoft.EntityFrameworkCore;
using System.Net.Mail;
using System.Net;

using static System.Runtime.InteropServices.JavaScript.JSType;
using InstagramWebAPI.Common;
using System.Globalization;

namespace InstagramWebAPI.BLL
{
    public class AuthService : IAuthService
    {
        public readonly ApplicationDbContext _dbcontext;
        public readonly IJWTService _jWTService;

        public AuthService(ApplicationDbContext db, IConfiguration configuration, IJWTService jWTService)
        {
            _dbcontext = db;
            _jWTService = jWTService;
        }

        /// <summary>
        /// Checks if the given username is unique.
        /// </summary>
        /// <param name="userName">The username to check.</param>
        /// <returns>True if the username is unique; false otherwise.</returns>
        public async Task<bool> IsUniqueUserName(string userName)
        {
            User? user = await _dbcontext.Users.FirstOrDefaultAsync(m => m.UserName == userName && m.IsDeleted != true);
            if (user == null) return false;

            return true;
        }

        /// <summary>
        /// Checks if the given username is unique.
        /// </summary>
        /// <param name="userName">The username to check.</param>
        /// <returns>True if the username is unique; false otherwise.</returns>
        public async Task<bool> IsUniqueUserNameEmailPhoneNumber(UserDTO model)
        {
            User? user = await _dbcontext.Users.FirstOrDefaultAsync(m => ((m.UserName ?? string.Empty).ToLower() == (model.UserName ?? string.Empty).ToLower() && !string.IsNullOrWhiteSpace(m.UserName)
                                       || (m.Email ?? string.Empty).ToLower() == (model.EmailOrNumber ?? string.Empty).ToLower() && !string.IsNullOrWhiteSpace(m.Email)
                                       || m.ContactNumber == model.EmailOrNumber && !string.IsNullOrWhiteSpace(m.ContactNumber)
                                       || (m.Email ?? string.Empty).ToLower() == (model.Email ?? string.Empty).ToLower() && !string.IsNullOrWhiteSpace(m.Email)
                                       || m.ContactNumber == model.ContactNumber && !string.IsNullOrWhiteSpace(m.ContactNumber))
                                       && m.IsDeleted != true);
            if (user == null) return false;

            return true;
        }

        /// <summary>
        /// Registers a new user asynchronously.
        /// </summary>
        /// <param name="model">The registration details.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains the registered User.</returns>
        public async Task<User> UpSertUserAsync(UserDTO model)
        {
            try
            {
                User user = _dbcontext.Users.FirstOrDefault(m => m.UserId == model.UserId && m.IsDeleted != true) ?? new();

                if (string.IsNullOrWhiteSpace(model.EmailOrNumber))
                {
                    user.Email = model.Email ?? string.Empty;
                    user.ContactNumber = model.ContactNumber ?? string.Empty;
                }
                else
                {
                    if (model.EmailOrNumber == "phone")
                    {
                        user.ContactNumber = model.EmailOrNumber ?? string.Empty;
                    }
                    else
                    {
                        user.Email = model.EmailOrNumber ?? string.Empty;
                    }
                }
                user.Name = model.Name ?? string.Empty;
                user.Password = BCrypt.Net.BCrypt.HashPassword(model.Password);
                user.UserName = model.UserName ?? string.Empty;
                user.CreatedDate = DateTime.Now;
                user.DateOfBirth = DateTime.TryParseExact(model.DateOfBirth, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime dob) ? dob : DateTime.MinValue;
                user.Bio = model.Bio;
                user.Link = model.Link;
                user.Gender = model.Gender;

                if (user.UserId > 0)
                {
                    user.ModifiedDate = DateTime.Now;
                    _dbcontext.Users.Update(user);
                }
                else
                {
                    user.CreatedDate = DateTime.Now;
                    await _dbcontext.Users.AddAsync(user);
                }

                await _dbcontext.SaveChangesAsync();

                user.Password = "";
                return user;
            }
            catch
            {
                throw new Exception(CustomErrorMessage.RegistrationError);
            }
        }

        /// <summary>
        /// Authenticates a user based on login credentials asynchronously.
        /// </summary>
        /// <param name="model">The login credentials.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains a LoginResponseDTO.</returns>
        public async Task<LoginResponseDTO> UserLoginAsync(LoginRequestDTO model)
        {
            try
            {
                User? user = await _dbcontext.Users.FirstOrDefaultAsync(m =>
                                       ((m.UserName ?? string.Empty).ToLower() == (model.UserID ?? string.Empty).ToLower() && !string.IsNullOrWhiteSpace(m.UserName)
                                       || (m.Email ?? string.Empty).ToLower() == (model.UserID ?? string.Empty).ToLower() && !string.IsNullOrWhiteSpace(m.Email)
                                       || m.ContactNumber == model.UserID && !string.IsNullOrWhiteSpace(m.ContactNumber))
                                       && m.IsDeleted != true);

                if (user == null)
                {
                    return new LoginResponseDTO
                    {
                        Token = "",
                        User = null,
                    };
                }
                else
                {
                    if (!BCrypt.Net.BCrypt.Verify(model.Password, user.Password))
                    {
                        return new LoginResponseDTO
                        {
                            Token = "",
                            User = null,
                        };
                    }
                }
                user.Password = "";
                LoginResponseDTO loginResponceDTO = new()
                {
                    Token = _jWTService.GetJWTToken(user),
                    User = user,
                };

                return loginResponceDTO;
            }
            catch
            {
                throw new Exception(CustomErrorMessage.LoginError);
            }
        }

        /// <summary>
        /// Retrieves a user asynchronously based on provided reset password data.
        /// </summary>
        /// <param name="model">The data containing email, mobile number, or username for user retrieval.</param>
        /// <returns>A <see cref="Task{TResult}"/> representing the asynchronous operation. The task result is the <see cref="User"/> entity.</returns>
        public async Task<User> GetUser(ForgotPasswordDTO model)
        {
            User? user = await _dbcontext.Users.FirstOrDefaultAsync(m => (m.Email == model.EmailOrNumberOrUserName && !string.IsNullOrWhiteSpace(m.Email)
                                                                      || m.ContactNumber == model.EmailOrNumberOrUserName && !string.IsNullOrWhiteSpace(m.ContactNumber)
                                                                      || m.UserName == model.EmailOrNumberOrUserName && !string.IsNullOrWhiteSpace(m.UserName))
                                                                      && m.IsDeleted != true);
            if (user != null)
                return user;

            throw new ValidationException(CustomErrorMessage.ExitsUser, CustomErrorCode.IsNotExits, new List<ValidationError>
               {
                       new ValidationError
                    {
                        message = CustomErrorMessage.ExitsUser,
                        reference = "UserName",
                        parameter = "UserName",
                        errorCode = CustomErrorCode.IsNotExits
                    }
               });
        }

        /// <summary>
        /// Updates the password for a user who has forgotten their password.
        /// </summary>
        /// <param name="model">The data containing the user ID and new password.</param>
        /// <returns>A <see cref="Task{TResult}"/> representing the asynchronous operation. The task result indicates if the password was successfully updated.</returns>
        public async Task<bool> ForgotPasswordAsync(ForgotPasswordDTO model)
        {
            try
            {
                User? user = await _dbcontext.Users.FirstOrDefaultAsync(m => m.UserId == model.UserId && m.IsDeleted != true);
                if (user != null)
                {
                    user.Password = BCrypt.Net.BCrypt.HashPassword(model.Password);
                    user.ModifiedDate = DateTime.Now;

                    _dbcontext.Users.Update(user);
                    await _dbcontext.SaveChangesAsync();

                    return true;
                }
                return false;
            }
            catch { return false; }
        }

        public async Task<bool> ResetPasswordAsync(ResetPasswordRequestDTO model)
        {
            try
            {
                User? user = await _dbcontext.Users.FirstOrDefaultAsync(m => m.UserId == model.UserId && m.IsDeleted != true);
                if (user != null)
                {
                    user.Password = BCrypt.Net.BCrypt.HashPassword(model.Password);
                    user.ModifiedDate = DateTime.Now;

                    _dbcontext.Users.Update(user);
                    await _dbcontext.SaveChangesAsync();

                    return true;
                }
                return false;
            }
            catch { return false; }
        }
    }
}
