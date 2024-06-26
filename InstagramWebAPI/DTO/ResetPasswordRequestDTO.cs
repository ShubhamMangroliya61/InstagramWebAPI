namespace InstagramWebAPI.DTO
{
    public class ResetPasswordRequestDTO
    {
        public long UserId { get; set; }
        public string? OldPassword { get; set; }
        public string? Password { get; set; }
        public string? ConfirmPassword { get; set; }
    }
}
