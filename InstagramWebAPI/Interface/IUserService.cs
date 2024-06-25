using InstagramWebAPI.DTO;

namespace InstagramWebAPI.Interface
{
    public interface IUserService
    {
        Task<ProfilePhotoResponseDTO> UploadProfilePhotoAsync(UploadProfilePhotoDTO model);
        Task<UserDTO> UpdateProfileAsync(UserDTO model);
        Task<bool> FollowRequestAsync(FollowRequestDTO model);
    }
}
