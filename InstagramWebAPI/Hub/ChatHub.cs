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
            List<long> messId=await _chatService.IsDelivredMessages(Int32.Parse(userId));
            await Clients.All.SendAsync("UserConnected", userId, messId);
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

        public async Task<MessageDTO> SendMessageToUser(long toUserId, string message,long chatId)
        {
            ConnectedUsers.TryGetValue(toUserId.ToString(), out var connectionId);
            MessageReqDTO messReq = new ()
            {
                ChatId = chatId,
                ToUserId = toUserId,
                FromUserId = Int32.Parse(GetClaimUserId()),
                Messages = message,
                IsDeliverd = connectionId != null ? true : false
            };

            MessageDTO messageDTO =await _chatService.SaveMessagesAsync(messReq);

            if (connectionId != null)
            {
                await _hubContext.Clients.Client(connectionId).SendAsync("ReceiveMessage", messageDTO);
            }
            return messageDTO;
        }

        public async Task<ChatDTO> CreateChat(long toUserId)
        {
            ConnectedUsers.TryGetValue(toUserId.ToString(), out var connectionId);

            ChatDTO chatDTO = await _chatService.CreateChatAsync(Int32.Parse(GetClaimUserId()),toUserId);

            if (connectionId != null)
            {
                await _hubContext.Clients.Client(connectionId).SendAsync("ReceiveMessage", chatDTO);
            }

            return chatDTO;
        }

        public async Task MarkAsReadMessages(long userId,long chatId)
        {
            await _chatService.MarkAsReadMessages(userId, chatId);
            if (ConnectedUsers.TryGetValue(userId.ToString(), out var connectionId))
            {
                await _hubContext.Clients.Client(connectionId).SendAsync("MarkAsRead", userId, chatId);
            }
        }

        public static string? GetConnectionId(string userId)
        {
            ConnectedUsers.TryGetValue(userId, out var connectionId);
            return connectionId;
        }
    }
}
