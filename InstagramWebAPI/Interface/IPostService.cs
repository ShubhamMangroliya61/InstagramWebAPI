using DataAccess.CustomModel;
using InstagramWebAPI.Common;
using InstagramWebAPI.DTO;

namespace InstagramWebAPI.Interface
{
    public interface IPostService
    {
        Task<PostResponseDTO> CreatePostAsync(CreatePostDTO model);
        Task<PaginationResponceModel<PostResponseDTO>> GetPostsListByIdAsync(RequestDTO<PostListRequestDTO> model);
        Task<PaginationResponceModel<PostResponseDTO>> GetPostListByUserIdAsync(RequestDTO<PostRequestDTO> model);
        Task<bool> DetelePostAsync(long postId);
        Task<bool> LikeAndUnlikePostAsync(LikePostDTO model);
        Task<bool> CommentPostAsync(CommentPostDTO model);
        Task<bool> DetelePostCommentAsync(long commentId);
        Task<PostResponseDTO> GetPostById(long postId, string postType);
        Task<CollectionDTO> UpsertCollectionAsync(CollectionRequestDTO model);
        Task<bool> DeteleCollectionAsync(long colletionId);
        Task<bool> AddPostCollectionAsync(long collectionId, long postId);
        Task<bool> DeletePostCollectionAsync(long postCollectionId);
        Task<PaginationResponceModel<CollectionDTO>> GetcollectionListByUserId(RequestDTO<UserIdRequestDTO> model);
    }
}
