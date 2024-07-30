using InstagramWebAPI.DTO;
using InstagramWebAPI.Interface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using System;
using System.Collections.Concurrent;
using System.IdentityModel.Tokens.Jwt;
using System.Threading.Tasks;

namespace ChatApp.Hubs
{
    public class ChatHub : Hub
    {
        private readonly IHubContext<ChatHub> _hubContext;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IChatService _chatService;

        private static readonly ConcurrentDictionary<string, string> ConnectedUsers = new ConcurrentDictionary<string, string>();

        public ChatHub(IHubContext<ChatHub> hubContext, IHttpContextAccessor httpContextAccessor, IChatService chatService)
        {
            _hubContext = hubContext;
            _httpContextAccessor = httpContextAccessor;
            _chatService = chatService;
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
            string userId = GetClaimUserId();
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

        public async Task SendMessageToUser(long toUserId, string message,long chatId)
        {
            if (ConnectedUsers.TryGetValue(toUserId.ToString(), out var connectionId))
            {
                MessageDTO messageDTO =await _chatService.SaveMessagesAsync(toUserId, message,chatId);
                await _hubContext.Clients.Client(connectionId).SendAsync("ReceiveMessage", messageDTO);
            }
            else
            {
                // Handle scenario where user is not connected
            }
        }

        public string? GetConnectionId(string userId)
        {
            ConnectedUsers.TryGetValue(userId, out var connectionId);
            return connectionId;
        }
    }
}
