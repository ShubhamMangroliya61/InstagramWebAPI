using DataAccess.CustomModel;
using InstagramWebAPI.DTO;

namespace InstagramWebAPI.Interface
{
    public interface IPostService
    {
        Task<PostResponseDTO> CreatePostAsync(CreatePostDTO model);
        Task<PaginationResponceModel<PostResponseDTO>> GetPostsListByIdAsync(RequestDTO<PostListRequestDTO> model);
        Task<bool> DetelePostAsync(long postId);
        Task<bool> LikeAndUnlikePostAsync(LikePostDTO model);
    }
}
