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
    [Route("api/Notification")]
    [ApiController]
    public class NotifiationContoller : ControllerBase
    {
        private readonly IValidationService _validationService;
        private readonly INotificationService _notificationService;
        private readonly ResponseHandler _responseHandler;

        public NotifiationContoller(IValidationService validationService, ResponseHandler responseHandler, INotificationService notificationService)
        {
            _validationService = validationService;
            _responseHandler = responseHandler;
            _notificationService = notificationService;
        }

        [HttpPost("GetNotificationListById")]
        [Authorize]
        public async Task<ActionResult> GetNotificationListByIdAsync([FromBody] PaginationRequestDTO model)
        {
            try
            {
                PaginationResponceModel<NotificationResponseDTO> data = await _notificationService.GetNotificationListById(model);
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

        [HttpPost("DeleteNotification")]
        [Authorize]
        public async Task<ActionResult> DeleteNotificationAsync(List<long> notificationId)
        {
            try
            {
                List<ValidationError> errors = _validationService.ValidateNotificationIds(notificationId);
                if (errors.Any())
                {
                    return BadRequest(_responseHandler.BadRequest(CustomErrorCode.IsValid, CustomErrorMessage.ValidationNotification, errors));
                }
                bool isDeleted = await _notificationService.DeteleNotificationAsync(notificationId);
                if (!isDeleted)
                {
                    return BadRequest(_responseHandler.BadRequest(CustomErrorCode.IsNotificationDelete, CustomErrorMessage.NotificationDeleteError, ""));
                }
                return Ok(_responseHandler.Success(CustomErrorMessage.NotificationDelete, notificationId));
            }
            catch (Exception ex)
            {
                if (ex is ValidationException vx)
                {
                    return BadRequest(_responseHandler.BadRequest(vx.ErrorCode, vx.Message, vx.Errors));
                }
                else
                {
                    return BadRequest(_responseHandler.BadRequest(CustomErrorCode.IsNotificationDelete, ex.Message, ""));
                }
            }
        }
    }
}
