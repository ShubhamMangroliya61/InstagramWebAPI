namespace InstagramWebAPI.DTO
{
    public class ForgotPasswordDTO
    {
        public long UserId { get; set; }
        public string? EmailOrNumberOrUserName { get; set; }
        public string? Password { get; set; }
        public string? ConfirmPassword { get; set; }
        public string? Type { get; set; }
    }
}
