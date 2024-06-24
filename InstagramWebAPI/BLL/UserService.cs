﻿using AutoMapper;
using InstagramWebAPI.DAL.Models;
using InstagramWebAPI.DTO;
using InstagramWebAPI.Interface;
using InstagramWebAPI.Utils;
using Microsoft.EntityFrameworkCore;
using System.Net.Mail;
using System.Net;

namespace InstagramWebAPI.BLL
{
    public class UserService : IUserService
    {
        public readonly ApplicationDbContext _dbcontext;
        public readonly IJWTService _jWTService;
        private readonly IMapper _mapper;

        public UserService(ApplicationDbContext db, IConfiguration configuration, IJWTService jWTService, IMapper mapper)
        {
            _dbcontext = db;
            _jWTService = jWTService;
            _mapper = mapper;
        }

        /// <summary>
        /// Checks if the given username is unique.
        /// </summary>
        /// <param name="userName">The username to check.</param>
        /// <returns>True if the username is unique; false otherwise.</returns>
        public async Task<bool> IsUniqueUserName(string userName)
        {
            User? user = await _dbcontext.Users.FirstOrDefaultAsync(m => m.UserName == userName);
            if (user == null) return false;

            return true;
        }

        /// <summary>
        /// Registers a new user asynchronously.
        /// </summary>
        /// <param name="model">The registration details.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains the registered User.</returns>
        public async Task<User> UserRegisterAsync(RegistrationRequestDTO model)
        {
            try
            {
                User user = new()
                {
                    Email = model.Email ?? string.Empty,
                    ContactNumber = model.MobileNumber ?? string.Empty,
                    Name = model.Name ?? string.Empty,
                    Password = BCrypt.Net.BCrypt.HashPassword(model.Password),
                    UserName = model.UserName ?? string.Empty,
                    CreatedDate = DateTime.Now,
                    Gender = "male"

                };
                await _dbcontext.Users.AddAsync(user);
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
                                       ((m.UserName ?? string.Empty).ToLower() == (model.UserName ?? string.Empty).ToLower()
                                       || (m.Email ?? string.Empty).ToLower() == (model.UserName ?? string.Empty).ToLower()
                                       || m.ContactNumber == model.MobileNumber)
                                       && m.IsDeleted != true);

                if (!BCrypt.Net.BCrypt.Verify(model.Password, user.Password) && user == null)
                {
                    return new LoginResponseDTO
                    {
                        Token = "",
                        User = null,
                    };
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
        public async Task<User> GetUser(ResetPasswordDTO model)
        {
            User? user = await _dbcontext.Users.FirstOrDefaultAsync(m => (m.Email == model.Email 
                                                                      || m.ContactNumber == model.MobileNumber && !string.IsNullOrWhiteSpace(m.ContactNumber) 
                                                                      || m.UserName == model.UserName) 
                                                                      && m.IsDeleted != true);
            if (user != null) 
                return user;

             throw new Exception(CustomErrorMessage.ExitsUser);
        }

        /// <summary>
        /// Updates the password for a user who has forgotten their password.
        /// </summary>
        /// <param name="model">The data containing the user ID and new password.</param>
        /// <returns>A <see cref="Task{TResult}"/> representing the asynchronous operation. The task result indicates if the password was successfully updated.</returns>
        public async Task<bool> ForgotPasswordData(ResetPasswordDTO model)
        {
            try
            {
                User? user = await _dbcontext.Users.FirstOrDefaultAsync(m => m.UserId == model.UserId && m.IsDeleted != true);
                if (user != null)
                {
                    user.Password = model.Password ?? string.Empty;
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
