using AutoMapper;
using InstagramWebAPI.DAL.Models;
using InstagramWebAPI.DTO;
using InstagramWebAPI.Interface;
using InstagramWebAPI.Utils;
using Microsoft.EntityFrameworkCore;

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
        /// Uploads a profile photo for a user asynchronously.
        /// </summary>
        /// <param name="model">The model containing UserId and the profile photo to upload.</param>
        /// <returns>A ProfilePhotoResponseDTO containing the uploaded photo details.</returns>
        public async Task<ProfilePhotoResponseDTO> UploadProfilePhotoAsync(UploadProfilePhotoDTO model)
        {
            try
            {
                User user = await _dbcontext.Users.FirstOrDefaultAsync(m => m.UserId == model.UserId && m.IsDeleted != true)
                            ?? throw new CustomException(CustomErrorMessage.ExitsUser);

                IFormFile file = model.ProfilePhoto ?? throw new CustomException(CustomErrorMessage.NullProfilePhoto);

                string userId = model.UserId.ToString();

                string path = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "content", "User",userId, "ProfilePhoto");

                if (!Directory.Exists(path))
                {
                    Directory.CreateDirectory(path);
                }

                string fileName = Guid.NewGuid().ToString() + Path.GetExtension(file.FileName);
                string filePath = Path.Combine(path, fileName);


                string currentFilePath = user.ProfilePictureUrl ?? string.Empty;
                string oldFileName=user.ProfilePictureName ?? string.Empty;
                if (System.IO.File.Exists(currentFilePath))
                {
                    System.IO.File.Delete(oldFileName);
                }

                using (var fileStream = new FileStream(filePath, FileMode.Create))
                {
                    await file.CopyToAsync(fileStream);
                }

                user.ProfilePictureUrl = Path.Combine("content", "User", userId, "ProfilePhoto", fileName);
                user.ProfilePictureName = fileName;

                _dbcontext.Users.Update(user);
                await _dbcontext.SaveChangesAsync();

                ProfilePhotoResponseDTO photoResponseDTO = new()
                {
                    ProfilePhotoName = user.ProfilePictureName,
                    ProfilePhotoUrl = user.ProfilePictureUrl,
                    UserId = user.UserId,
                };
                return photoResponseDTO;
            }
            catch (CustomException ex)
            {
                throw new CustomException(ex.ErrorMessage);
            }
            catch (Exception)
            {
                throw new Exception(CustomErrorMessage.UploadError);
            }
        }

        public async Task<UserDTO> UpdateProfileAsync(UserDTO model)
        {
            try
            {
                User user = await _dbcontext.Users.FirstOrDefaultAsync(m => m.UserId == model.UserId && m.IsDeleted != true)
                            ?? throw new CustomException(CustomErrorMessage.ExitsUser);

                user.Name= model.Name;
                user.UserName= model.UserName;
                user.Bio= model.Bio;
                user.Link= model.Link;
                user.Gender=model.Gender;

                _dbcontext.Users.Update(user);
                await _dbcontext.SaveChangesAsync();

                return _mapper.Map<UserDTO>(user);
            }
            catch (CustomException ex)
            {
                throw new CustomException(ex.ErrorMessage);
            }
            catch (Exception)
            {
                throw new Exception(CustomErrorMessage.UpdateProfile);
            }
        }

        public async Task<bool> FollowRequestAsync(FollowRequestDTO model)
        {
            try
            {
                if (!await _dbcontext.Users.AnyAsync(m => m.UserId == model.ToUserId) ||
                    !await _dbcontext.Users.AnyAsync(m => m.UserId == model.FromUserId))
                {
                    throw new CustomException(CustomErrorMessage.ExitsUser);
                }

                Request data = await _dbcontext.Requests.FirstOrDefaultAsync(m => m.FromUserId == model.FromUserId && m.ToUserId == model.ToUserId)
                             ?? new ();

                data.FromUserId = model.FromUserId;
                data.ToUserId = model.ToUserId;

                if (data.RequestId > 0 )
                {
                    data.ModifiedDate = DateTime.Now;
                    data.IsDeleted = !(data.IsDeleted);

                    _dbcontext.Requests.Update(data);
                }
                else
                {
                    data.CreatedDate = DateTime.Now;
                    await _dbcontext.Requests.AddAsync(data);
                }
                await _dbcontext.SaveChangesAsync();
                return true;    
            }
            catch (CustomException ex)
            {
                throw new CustomException(ex.ErrorMessage);
            }
            catch 
            {
                return false;
            }
        }
    }
}
