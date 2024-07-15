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
        /// <summary>
        /// Retrieves a paginated list of search records for the logged-in user.
        /// </summary>
        /// <param name="model">Pagination parameters including page number and page size.</param>
        /// <returns>
        /// A task that represents the asynchronous operation. 
        /// The task result contains a pagination response model with the list of search records.
        /// </returns>
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
        /// <summary>
        /// Deletes notifications identified by their IDs.
        /// </summary>
        /// <param name="notificationId">A list of notification IDs to delete.</param>
        /// <returns>
        /// An ActionResult representing the result of the operation.
        /// If successful, returns HTTP 200 (OK) with a success message.
        /// If validation fails or an error occurs, returns HTTP 400 (Bad Request) with an error message.
        /// </returns>
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
