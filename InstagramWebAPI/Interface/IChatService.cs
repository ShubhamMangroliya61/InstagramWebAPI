using DataAccess.CustomModel;
using InstagramWebAPI.DTO;

namespace InstagramWebAPI.Interface
{
    public interface IChatService
    {
        Task<ChatDTO> CreateChatAsync(long fromUserId, long ToUserId);
        Task<MessageDTO> SaveMessagesAsync(MessageReqDTO model);
        Task<List<long>> IsDelivredMessages(long userId);
        Task MarkAsReadMessages(long userId, long chatId);
        Task<PaginationResponceModel<ChatDTO>> GetChatListAsync(PaginationRequestDTO model);
        Task<PaginationResponceModel<MessageDTO>> GetMessagesListAsync(RequestDTO<MessagesReqDTO> model);

    }
}
