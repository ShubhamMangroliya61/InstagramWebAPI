namespace InstagramWebAPI.DTO
{
    public class UserDTO
    {
        public long UserId { get; set; }
        public string UserName { get; set; } = null!;
        public string? Name { get; set; }
        public string? Email { get; set; }
        public string? Gender { get; set; } 
        public string? ContactNumber { get; set; }
        public string? DateOfBirth { get; set; }
        public string? ProfilePictureUrl { get; set; }
        public string? ProfilePictureName { get; set; }
        public string? Bio { get; set; }= "";
        public string? Link { get; set; }
        public bool IsVerified { get; set; }= false;
        public bool IsPrivate { get; set; }= false;
        public bool IsDeleted { get; set; } = false;
        public bool IsFollower { get; set; } = false;
        public bool IsFollowing { get; set; } = false;
        public bool IsRequest { get; set; } = false;
        public string? Password { get; set; }

        
    }
}
