using InstagramWebAPI.DTO;

namespace InstagramWebAPI.Common
{
    public class ResponseHandler
    {
        //Response #200
        public ResponseModel Success(string Message, Object Data)
        {
            return new ResponseModel
            {
                IsSuccess = true,
                Message = Message,
                Data = Data,
                StatusCode = StatusCodes.Status200OK
            };
        }

        //Response #400
        public ResponseModel BadRequest(string ErrorCode,string Message,Object Data)
        {
            return new ResponseModel
            {
                IsSuccess = false,
                Message = Message,
                Data = Data,
                StatusCode = StatusCodes.Status400BadRequest,
                ErrorCode = ErrorCode
            };
        }
    }
}
