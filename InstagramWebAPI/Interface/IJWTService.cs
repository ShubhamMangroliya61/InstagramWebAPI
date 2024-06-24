using InstagramWebAPI.DAL.Models;
using InstagramWebAPI.DTO;

namespace InstagramWebAPI.Interface
{
    public interface IJWTService
    {
        string GetJWTToken(User user);
    }
}
