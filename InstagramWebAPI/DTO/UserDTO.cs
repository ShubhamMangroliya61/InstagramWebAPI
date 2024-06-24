namespace InstagramWebAPI.DTO
{
    public class UserDTO
    {
        public long UserId { get; set; }
        public string UserName { get; set; } = null!;
        public string? Email { get; set; }
        public string Password { get; set; } = null!;
        public string Gender { get; set; } = null!;
        public string? ContactNumber { get; set; }
        public DateTime? DateOfBirth { get; set; }
        public string? ProfilePictureUrl { get; set; }
        public string? ProfilePictureName { get; set; }
        public string? Bio { get; set; }
        public string? Name { get; set; }
        public bool IsVerified { get; set; }
        public bool IsPrivate { get; set; }
        public bool IsDeleted { get; set; }
    }
}
