using InstagramWebAPI.Interface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using System;
using System.Collections.Concurrent;
using System.Threading.Tasks;

namespace NotificationApp.Hubs
{
    public class NotificationHub : Hub
    {
        private readonly IHubContext<NotificationHub> _hubContext;
        private readonly IUserid _httpContextAccessor;

        private static readonly ConcurrentDictionary<string, string> ConnectedUsers = new ConcurrentDictionary<string, string>();

        public NotificationHub(IHubContext<NotificationHub> hubContext, IUserid httpContextAccessor)
        {
            _hubContext = hubContext;
            _httpContextAccessor = httpContextAccessor;
        }
        [Authorize]
        public override async Task OnConnectedAsync()
        {
            string userId = _httpContextAccessor.GetUserIdClaim().ToString()??"4";
            ConnectedUsers[userId] = Context.ConnectionId;
            await Clients.All.SendAsync("UserConnected", userId);
            await base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            string userId = _httpContextAccessor.GetUserIdClaim().ToString() ?? "4";
            ConnectedUsers.TryRemove(userId, out _);
            await Clients.All.SendAsync("UserDisconnected", userId);

            if (exception != null)
            {
                // Log the exception (use a logging framework like Serilog or NLog)
                Console.WriteLine($"Disconnected with error: {exception.Message}");
            }

            await base.OnDisconnectedAsync(exception);
        }

        public async Task SendNotificationToUser(int toUserId, object notificationData)
        {
            if (ConnectedUsers.TryGetValue(toUserId.ToString(), out var connectionId))
            {
                await _hubContext.Clients.Client(connectionId).SendAsync("ReceiveNotification", notificationData);
            }
        }

        public string? GetConnectionId(string userId)
        {
            ConnectedUsers.TryGetValue(userId, out var connectionId);
            return connectionId;
        }
    }
}
