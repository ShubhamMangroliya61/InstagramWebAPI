namespace InstagramWebAPI.DTO
{
    public class ResetPasswordDTO
    {
        public long? UserId { get; set; }
        public string? Email { get; set; }
        public string? MobileNumber { get; set; }
        public string? UserName { get; set; }
        public string? Password { get; set; }
        public string? ConfirmPassword { get; set; }
    }
}
