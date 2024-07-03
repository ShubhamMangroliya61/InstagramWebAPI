using DataAccess.CustomModel;
using InstagramWebAPI.DTO;

namespace InstagramWebAPI.Interface
{
    public interface IStoryService
    {
        Task<StoryResponseListDTO> AddStoryAsync(AddStoryDTO model);
        Task<bool> DeteleStoryAsync(long storyId);
        Task<PaginationResponceModel<StoryResponseListDTO>> GetStoryListByUserIdAsync(RequestDTO<UserIdRequestDTO> model);
        Task<bool> StorySeenByUserIdAsync(long storyId);
        Task<StoryResponseListDTO> GetStoryById(long userId, long storyId);
        Task<bool> LikeStoryAsync(long storyId, bool isLike);
        Task<HighlightDTO> UpsertHighlightAsync(HighLightRequestDTO model);
        Task<bool> DeteleHighLightAsync(long highLightId);
        Task<bool> AddStoryHighLightAsync(long highLightId, long storyId);
        Task<bool> DeleteStoryHighLightAsync(long storyHighLightId);
        Task<PaginationResponceModel<HighlightDTO>> GetHighLightListByUserId(RequestDTO<UserIdRequestDTO> model);
    }
}
