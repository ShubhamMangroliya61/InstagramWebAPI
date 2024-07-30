using DataAccess.CustomModel;
using InstagramWebAPI.DTO;

namespace InstagramWebAPI.Interface
{
    public interface IChatService
    {
        Task<ChatDTO> CreateChatAsync(long ToUserId);
        Task<MessageDTO> SaveMessagesAsync(long toUserId, string messageText, long chatId);
        Task<PaginationResponceModel<ChatDTO>> GetChatListAsync(PaginationRequestDTO model);
        Task<PaginationResponceModel<MessageDTO>> GetMessagesListAsync(RequestDTO<MessagesReqDTO> model);

    }
}
