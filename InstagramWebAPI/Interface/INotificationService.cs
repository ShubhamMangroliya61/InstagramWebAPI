using DataAccess.CustomModel;
using InstagramWebAPI.DTO;

namespace InstagramWebAPI.Interface
{
    public interface INotificationService
    {
        Task<PaginationResponceModel<NotificationResponseDTO>> GetNotificationListById(PaginationRequestDTO model);
        Task<bool> DeteleNotificationAsync(List<long> notificationId);
    }
}
