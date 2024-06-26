using DataAccess.CustomModel;
using InstagramWebAPI.DTO;

namespace InstagramWebAPI.Interface
{
    public interface IUserService
    {
        Task<ProfilePhotoResponseDTO> UploadProfilePhotoAsync(UploadProfilePhotoDTO model);
       
        Task<bool> FollowRequestAsync(FollowRequestDTO model);
        Task<UserDTO> GetUserByIdAsync(long userId);
        Task<PaginationResponceModel<UserDTO>> GetFollowerORFollowingListAsync(RequestDTO<FollowerListRequestDTO> model);
        Task<PaginationResponceModel<RequestListResponseDTO>> GetRequestListByIdAsync(RequestDTO<FollowRequestDTO> model);
        Task<bool> RequestAcceptOrCancelAsync(long requestId, string acceptType);
        Task<CountResponseDTO> GetFollowerAndFollowingCountByIdAsync(long userId);

    }
}
