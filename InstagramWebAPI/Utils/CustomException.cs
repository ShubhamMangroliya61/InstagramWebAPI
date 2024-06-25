namespace InstagramWebAPI.Utils
{
    public class CustomException : Exception
    {
        public int ErrorCode { get; set; }
        public string ErrorMessage { get; set; }

        public CustomException(string message) : base(message)
        {
            ErrorMessage = message;
        }
        public CustomException(string message, int errorCode) : base(message)
        {
            ErrorCode = errorCode;
            ErrorMessage = message;
        }
    }
}
