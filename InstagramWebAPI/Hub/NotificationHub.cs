using InstagramWebAPI.Interface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using System;
using System.Collections.Concurrent;
using System.IdentityModel.Tokens.Jwt;
using System.Threading.Tasks;

namespace NotificationApp.Hubs
{
    public class NotificationHub : Hub
    {
        private readonly IHubContext<NotificationHub> _hubContext;
        private readonly IHttpContextAccessor _httpContextAccessor;


        private static readonly ConcurrentDictionary<string, string> ConnectedUsers = new ConcurrentDictionary<string, string>();

        public NotificationHub(IHubContext<NotificationHub> hubContext,IHttpContextAccessor httpContextAccessor)
        {
            _hubContext = hubContext;
            _httpContextAccessor = httpContextAccessor;
        }

        public override async Task OnConnectedAsync()
        {
            string userId = GetClaimUserId();
            ConnectedUsers[userId] = Context.ConnectionId;
            await Clients.All.SendAsync("UserConnected", userId);
            await base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            string userId =GetClaimUserId();
            ConnectedUsers.TryRemove(userId, out _);
            await Clients.All.SendAsync("UserDisconnected", userId);

            if (exception != null)
            {
                Console.WriteLine($"Disconnected with error: {exception.Message}");
            }

            await base.OnDisconnectedAsync(exception);
        }

        public string GetClaimUserId()
        {
            var jwtToken = _httpContextAccessor.HttpContext?.Request.Query["access_token"].FirstOrDefault()?.Split(" ").LastOrDefault();
            var handler = new JwtSecurityTokenHandler();
            var token = handler.ReadJwtToken(jwtToken);
            var claims = token.Claims.Select(claim => (claim.Type, claim.Value)).ToList();

            return claims.FirstOrDefault(m => m.Type == "UserId").Value;
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
