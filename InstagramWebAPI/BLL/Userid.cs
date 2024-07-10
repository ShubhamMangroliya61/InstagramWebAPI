using InstagramWebAPI.DAL.Models;
using InstagramWebAPI.Interface;
using NotificationApp.Hubs;

namespace InstagramWebAPI.BLL
{
    public class Userid : IUserid
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public Userid(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;

        }
        public long GetUserIdClaim()
        {
            var xyz = _httpContextAccessor.HttpContext.Request;
            var userIdClaim = _httpContextAccessor?.HttpContext?.User.FindFirst("UserId");

            if (userIdClaim != null && long.TryParse(userIdClaim.Value, out long userId))
            {
                return userId;
            }
            return 0;
        }
    }
}
