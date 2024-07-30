using DataAccess.CustomModel;
using InstagramWebAPI.DAL.Models;
using InstagramWebAPI.DTO;
using InstagramWebAPI.Helpers;
using InstagramWebAPI.Interface;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;

namespace InstagramWebAPI.BLL
{
    public class ChatService : IChatService
    {
        public readonly ApplicationDbContext _dbcontext;
        private readonly Helper _helper;

        public ChatService(ApplicationDbContext db, Helper helper)
        {
            _dbcontext = db;
            _helper = helper;
        }

        public async Task<ChatDTO> CreateChatAsync(long ToUserId)
        {
            long UserId = _helper.GetUserIdClaim();

            Chat? chat = await _dbcontext.Chats.FirstOrDefaultAsync(m => (m.FromUserId == UserId && m.ToUserId == ToUserId) || (m.ToUserId == UserId && m.FromUserId == ToUserId));
            Chat obj = chat ?? new();

            if (chat == null)
            {
                obj.FromUserId = UserId;
                obj.ToUserId = ToUserId;
               await _dbcontext.Chats.AddAsync(obj);
            }
            else
            {
                obj.IsDeleted = true;
            }
            await _dbcontext.SaveChangesAsync();

            return new ChatDTO
            {
                ChatId = obj.ChatId,
                ToUserId = obj.ToUserId,
                ToUserName = (await _dbcontext.Users.FindAsync(obj.ToUserId))?.UserName ?? string.Empty,
                ProfileName = (await _dbcontext.Users.FindAsync(obj.ToUserId))?.ProfilePictureName ?? string.Empty,
                CreatedDate = obj.CreatedDate
            };
        }

        public async Task<PaginationResponceModel<ChatDTO>> GetChatListAsync(PaginationRequestDTO model)
        {
            long UserId = _helper.GetUserIdClaim();

            IQueryable<ChatDTO> fromChats = _dbcontext.Chats.Where(m => m.IsDeleted == false && m.FromUserId == UserId).Include(m => m.ToUser)
                .Select(m => new ChatDTO
                {
                    ChatId = m.ChatId,
                    ToUserId = m.ToUser.UserId,
                    ToUserName = m.ToUser.UserName,
                    ProfileName = m.ToUser.ProfilePictureName,
                    CreatedDate = m.CreatedDate,
                });

            IQueryable<ChatDTO> toChats = _dbcontext.Chats.Where(m => m.IsDeleted == false && m.ToUserId == UserId).Include(m => m.FromUser)
                 .Select(m => new ChatDTO
                 {
                     ChatId = m.ChatId,
                     ToUserId = m.FromUser.UserId,
                     ToUserName = m.FromUser.UserName,
                     ProfileName = m.FromUser.ProfilePictureName,
                     CreatedDate = m.CreatedDate,
                 });

            IQueryable<ChatDTO> allChats = fromChats.Concat(toChats).Distinct().OrderByDescending(m=>m.CreatedDate);

            int totalRecords = await allChats.CountAsync();
            int requiredPages = (int)Math.Ceiling((decimal)totalRecords / model.PageSize);

            List<ChatDTO> chats = await allChats
                .Skip((model.PageNumber - 1) * model.PageSize)
                .Take(model.PageSize)
                .ToListAsync();

            return new PaginationResponceModel<ChatDTO>
            {
                Totalrecord = totalRecords,
                PageSize = model.PageSize,
                PageNumber = model.PageNumber,
                RequirdPage = requiredPages,
                Record = chats
            };
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

        public async Task<MessageDTO> SaveMessagesAsync(long toUserId, string messageText, long chatId)
        {
            long userId = _helper.GetUserIdClaim();

            Message message = new Message
            {
                ChatId = chatId,
                FromUserId = userId,
                ToUserId = toUserId,
                MessageText = messageText,
                CreatedDate = DateTime.UtcNow 
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
                CreatedDate = message.CreatedDate
            };
        }

    }
}
