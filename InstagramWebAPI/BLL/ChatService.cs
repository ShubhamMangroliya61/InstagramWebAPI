using ChatApp.Hubs;
using DataAccess.CustomModel;
using InstagramWebAPI.Common;
using InstagramWebAPI.DAL.Models;
using InstagramWebAPI.DTO;
using InstagramWebAPI.Helpers;
using InstagramWebAPI.Interface;
using InstagramWebAPI.Utils;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using System.Threading;

namespace InstagramWebAPI.BLL
{
    public class ChatService : IChatService
    {
        public readonly ApplicationDbContext _dbcontext;
        private readonly IHubContext<ChatHub> _hubContext;
        private readonly Helper _helper;

        public ChatService(ApplicationDbContext db, Helper helper, IHubContext<ChatHub> hubContext)
        {
            _dbcontext = db;
            _hubContext = hubContext;
            _helper = helper;
        }

        public async Task<ChatDTO> CreateChatAsync(long fromUserId,long ToUserId)
        {
            //long UserId = _helper.GetUserIdClaim();

            Chat? chat = await _dbcontext.Chats.FirstOrDefaultAsync(m => (m.FromUserId == fromUserId && m.ToUserId == ToUserId) || (m.ToUserId == fromUserId && m.FromUserId == ToUserId));
            Chat obj = chat ?? new();

            if (chat == null)
            {
                obj.FromUserId = fromUserId;
                obj.ToUserId = ToUserId;
                await _dbcontext.Chats.AddAsync(obj);
            await _dbcontext.SaveChangesAsync();
            }
           

            return new ChatDTO
            {
                ChatId = obj.ChatId,
                ToUserId = obj.ToUserId,
                ToUserName = (await _dbcontext.Users.FindAsync(obj.ToUserId))?.UserName ?? string.Empty,
                ProfileName = (await _dbcontext.Users.FindAsync(obj.ToUserId))?.ProfilePictureName ?? string.Empty,
                CreatedDate = obj.CreatedDate
            };
        }

        public async Task<List<long>> IsDelivredMessages(long userId)
        {
            List<Message> messages=_dbcontext.Messages.Where(m=>m.IsDeleted == false && m.IsSeen == false && m.IsDelivered == false && m.ToUserId == userId).ToList();

            messages.ForEach(x =>
            {
                x.IsDelivered = true;
            });
            await _dbcontext.SaveChangesAsync();

            List<long> messageIds = messages.Select(m=>m.MessageId).ToList();
            return messageIds;
        }

        public async Task MarkAsReadMessages(long userId,long chatId)
        {
            List<Message> messages = _dbcontext.Messages.Where(m => m.IsDeleted == false && m.IsSeen == false && m.FromUserId == userId && m.ChatId == chatId).ToList();

            messages.ForEach(x =>
            {
                x.IsSeen = true;
            });
            await _dbcontext.SaveChangesAsync();
        }

        public async Task<PaginationResponceModel<ChatDTO>> GetChatListAsync(PaginationRequestDTO model)
        {
            long UserId = _helper.GetUserIdClaim();

            IQueryable<ChatDTO> fromChats = _dbcontext.Chats.Include(m=>m.Messages).Where(m => m.IsDeleted == false && m.FromUserId == UserId).Include(m => m.ToUser)
                .Select(m => new ChatDTO
                {
                    ChatId = m.ChatId,
                    ToUserId = m.ToUser.UserId,
                    ToUserName = m.ToUser.UserName,
                    ProfileName = m.ToUser.ProfilePictureName,
                    CreatedDate = m.CreatedDate,
                    LastMessage = m.Messages.OrderByDescending(m=>m.CreatedDate).First().MessageText,
                    Unread = m.Messages.Count(msg => msg.IsSeen == false && msg.IsDelivered == true)
                });

            IQueryable<ChatDTO> toChats = _dbcontext.Chats.Include(m=>m.Messages).Where(m => m.IsDeleted == false && m.ToUserId == UserId).Include(m => m.FromUser)
                 .Select(m => new ChatDTO
                 {
                     ChatId = m.ChatId,
                     ToUserId = m.FromUser.UserId,
                     ToUserName = m.FromUser.UserName,
                     ProfileName = m.FromUser.ProfilePictureName,
                     CreatedDate = m.CreatedDate,
                     LastMessage = m.Messages.OrderByDescending(m => m.CreatedDate).First().MessageText,
                     Unread = m.Messages.Count(msg => msg.IsSeen == false && msg.IsDelivered == true)
                 });

            IQueryable<ChatDTO> allChats = fromChats.Concat(toChats).Distinct().OrderByDescending(m => m.CreatedDate);

            int totalRecords = await allChats.CountAsync();
            int requiredPages = (int)Math.Ceiling((decimal)totalRecords / model.PageSize);

            List<ChatDTO> chats = await allChats
                .Skip((model.PageNumber - 1) * model.PageSize)
                .Take(model.PageSize)
                .ToListAsync();

            List<ChatDTO> chatDTOs = chats.Select(c => new ChatDTO
            {
                ChatId = c.ChatId,
                ToUserId = c.ToUserId,
                ToUserName = c.ToUserName,
                CreatedDate = c.CreatedDate,
                ProfileName = GetProfileImage(c.ToUserId, c.ProfileName ?? ""),
                LastMessage = c.LastMessage,
                Unread = c.Unread,
                IsOnline = !string.IsNullOrEmpty( ChatHub.GetConnectionId(c.ToUserId.ToString()))
            }).ToList();

            return new PaginationResponceModel<ChatDTO>
            {
                Totalrecord = totalRecords,
                PageSize = model.PageSize,
                PageNumber = model.PageNumber,
                RequirdPage = requiredPages,
                Record = chatDTOs
            };
        }
        private string GetProfileImage(long userId, string imageName)
        {
            int index = imageName.IndexOf('.') + 1;
            string extension = imageName[index..];
            string imagePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "content", "User", userId.ToString(), "ProfilePhoto", imageName);
            if (!File.Exists(imagePath))
            {
               return string.Empty;
            }
            byte[] imageBytes = System.IO.File.ReadAllBytes(imagePath);
            string base64String = Convert.ToBase64String(imageBytes);

            return base64String;
        }
        public async Task<PaginationResponceModel<MessageDTO>> GetMessagesListAsync(RequestDTO<MessagesReqDTO> model)
        {
            IQueryable<MessageDTO> messages = _dbcontext.Messages.Where(m => m.IsDeleted == false && m.ChatId == model.Model.ChatId)
                .Select(m => new MessageDTO
                {
                    MessagesId = m.MessageId,
                    ChatId = m.ChatId,
                    ToUserId = m.ToUserId,
                    FromUserId = m.FromUserId,
                    MessageText = m.MessageText,
                    IsSeen = m.IsSeen,
                    IsDeliverd =m.IsDelivered,
                    CreatedDate = m.CreatedDate,
                });

            int totalRecords = await messages.CountAsync();
            int requiredPages = (int)Math.Ceiling((decimal)totalRecords / model.PageSize);

            List<MessageDTO> messages1 = await messages
                .Skip((model.PageNumber - 1) * model.PageSize)
                .Take(model.PageSize)
                .ToListAsync();

            return new PaginationResponceModel<MessageDTO>
            {
                Totalrecord = totalRecords,
                PageSize = model.PageSize,
                PageNumber = model.PageNumber,
                RequirdPage = requiredPages,
                Record = messages1
            };
        }

        public async Task<MessageDTO> SaveMessagesAsync(MessageReqDTO model)
        {
            Message message = new Message
            {
                ChatId = model.ChatId,
                FromUserId = model.FromUserId,
                ToUserId = model.ToUserId,
                MessageText = model.Messages??"",
                IsDelivered = model.IsDeliverd
            };

            await _dbcontext.Messages.AddAsync(message);
            await _dbcontext.SaveChangesAsync();

            return new MessageDTO
            {
                MessagesId = message.MessageId,
                ChatId = message.ChatId,
                ToUserId = message.ToUserId,
                FromUserId = message.FromUserId,
                MessageText = message.MessageText,
                IsSeen = message.IsSeen,
                IsDeliverd = model.IsDeliverd,
                CreatedDate = message.CreatedDate
            };
        }

    }
}
