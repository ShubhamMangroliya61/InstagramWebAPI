using InstagramWebAPI.DAL.Models;

namespace InstagramWebAPI.DTO
{
    public class LoginResponseDTO
    {
        public User? User { get; set; }
        public string? Token { get; set; }
    }
}
