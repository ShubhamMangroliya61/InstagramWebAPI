using DataAccess.CustomModel;
using InstagramWebAPI.Common;
using InstagramWebAPI.DTO;

namespace InstagramWebAPI.Interface
{
    public interface IUserService
    {
        Task<ProfilePhotoResponseDTO> UploadProfilePhotoAsync(IFormFile ProfilePhoto, long userId);
        Task<bool> FollowRequestAsync(FollowRequestDTO model, long FromUserId);
        Task<UserDTO> GetUserByIdAsync(long userId);
        Task<PaginationResponceModel<UserDTO>> GetFollowerORFollowingListAsync(RequestDTO<FollowerListRequestDTO> model);
        Task<PaginationResponceModel<RequestListResponseDTO>> GetRequestListByIdAsync(RequestDTO<FollowRequestDTO> model);
        Task<PaginationResponceModel<UserDTO>> GetUserListByUserNameAsync(RequestDTO<UserIdRequestDTO> model);
        Task<bool> RequestAcceptOrCancelAsync(long requestId, string acceptType);
        Task<CountResponseDTO> GetFollowerAndFollowingCountByIdAsync(long userId);
        Task<PaginationResponceModel<MutualFriendDTO>> GetMutualFriendsWithDetailsAsync(RequestDTO<UserIdRequestDTO> model);
        Task<PaginationResponceModel<UserDTO>> GetSuggestionListAsync(PaginationRequestDTO model);
        string GetContentType(string fileExtension);
        Task<bool> UpsertSearchUserById(long searchUserId);
        Task<bool> DeleteSearchUser(long searchId);
        Task<PaginationResponceModel<SearchDTO>> GetSearchUserList(PaginationRequestDTO model);
    }
}
