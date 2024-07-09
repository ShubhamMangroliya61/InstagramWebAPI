using Microsoft.AspNetCore.SignalR;
using System.Threading.Tasks;

namespace NotificationApp.Hubs
{
    public class NotificationHub : Hub
    {
        private readonly IHubContext<NotificationHub> _hubContext;

        public NotificationHub(IHubContext<NotificationHub> hubContext)
        {
            _hubContext = hubContext;
        }

        public async Task SendNotificationToUser(int toUserId, object notificationData)
        {
            await _hubContext.Clients.User(toUserId.ToString()).SendAsync("ReceiveNotification", notificationData);
        }
    }
}
