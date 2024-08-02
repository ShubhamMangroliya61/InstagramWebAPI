using DataAccess.CustomModel;
using InstagramWebAPI.BLL;
using InstagramWebAPI.Common;
using InstagramWebAPI.DTO;
using InstagramWebAPI.Helpers;
using InstagramWebAPI.Interface;
using InstagramWebAPI.Utils;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace InstagramWebAPI.Controllers
{
    [Route("api/Chat")]
    [ApiController]
    public class ChatController : ControllerBase
    {
        private readonly IValidationService _validationService;
        private readonly IChatService _chatService;
        private readonly ResponseHandler _responseHandler;
        private readonly Helper _helper;

        public ChatController(IValidationService validationService, ResponseHandler responseHandler, IChatService chatService, Helper helper)
        {
            _validationService = validationService;
            _responseHandler = responseHandler;
            _chatService = chatService;
            _helper = helper;
        }

        //[HttpPost("CreateChatAsync")]
        //[Authorize]
        //public async Task<ActionResult> CreatePostAsync([FromQuery]long toUserId)
        //{
        //    try
        //    {
        //        ChatDTO responseDTO = await _chatService.CreateChatAsync(toUserId);
        //        if (responseDTO == null)
        //        {
        //            return BadRequest(_responseHandler.BadRequest(CustomErrorCode.IsChat, CustomErrorMessage.ChatError, ""));
        //        }
        //        return Ok(_responseHandler.Success(CustomErrorMessage.CreateChat, responseDTO));
        //    }
        //    catch (Exception ex)
        //    {
        //        if (ex is ValidationException vx)
        //        {
        //            return BadRequest(_responseHandler.BadRequest(vx.ErrorCode, vx.Message, vx.Errors));
        //        }
        //        else
        //        {
        //            return BadRequest(_responseHandler.BadRequest(CustomErrorCode.IsChat, ex.Message, ""));
        //        }
        //    }
        //}

        [HttpPost("GetChatListAsync")]
        [Authorize]
        public async Task<ActionResult> GetChatListAsync([FromBody] PaginationRequestDTO model)
        {
            try
            {
                PaginationResponceModel<ChatDTO> data = await _chatService.GetChatListAsync(model);
                if (data == null)
                {
                    return BadRequest(_responseHandler.BadRequest(CustomErrorCode.IsGetLIst, CustomErrorMessage.GetFollowerList, ""));
                }
                return Ok(_responseHandler.Success(CustomErrorMessage.GetFollowerListSucces, data));
            }
            catch (Exception ex)
            {
                if (ex is ValidationException vx)
                {
                    return BadRequest(_responseHandler.BadRequest(vx.ErrorCode, vx.Message, vx.Errors));
                }
                else
                {
                    return BadRequest(_responseHandler.BadRequest(CustomErrorCode.IsGetLIst, ex.Message, ""));
                }
            }
        }

        [HttpPost("GetMessagesListAsync")]
        [Authorize]
        public async Task<ActionResult> GetMessagesListAsync([FromBody] RequestDTO<MessagesReqDTO> model)
        {
            try
            {
                List<ValidationError> errors = _validationService.ValidateChatId(model.Model.ChatId);
                if (errors.Any())
                {
                    return BadRequest(_responseHandler.BadRequest(CustomErrorCode.IsValid, CustomErrorMessage.ValidationChat, errors));
                }
                PaginationResponceModel<MessageDTO> data = await _chatService.GetMessagesListAsync(model);
                if (data == null)
                {
                    return BadRequest(_responseHandler.BadRequest(CustomErrorCode.IsGetLIst, CustomErrorMessage.GetFollowerList, ""));
                }
                return Ok(_responseHandler.Success(CustomErrorMessage.GetFollowerListSucces, data));
            }
            catch (Exception ex)
            {
                if (ex is ValidationException vx)
                {
                    return BadRequest(_responseHandler.BadRequest(vx.ErrorCode, vx.Message, vx.Errors));
                }
                else
                {
                    return BadRequest(_responseHandler.BadRequest(CustomErrorCode.IsGetLIst, ex.Message, ""));
                }
            }
        }
    }
}
