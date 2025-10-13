using Microsoft.AspNetCore.Http;
using System.Security.Claims;
using WEB.SERVICES.IService;

namespace WEB.SERVICES.Service
{
    public class UserContextService : IUserContextService
    {
        private readonly IHttpContextAccessor _accessor;

        public UserContextService(IHttpContextAccessor accessor)
        {
            _accessor = accessor;
        }

        public string UserId => _accessor.HttpContext?.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? string.Empty;
        public string Role => _accessor.HttpContext?.User?.FindFirst(ClaimTypes.Role)?.Value ?? string.Empty;
        public string IpAddress => _accessor.HttpContext?.Connection?.RemoteIpAddress?.ToString() ?? "Unknown";
        public string DeviceInfo
        {
            get
            {
                var userAgent = _accessor.HttpContext?.Request?.Headers["User-Agent"].ToString();
                if (string.IsNullOrWhiteSpace(userAgent))
                    return "Unknown Device";

                return userAgent;
            }
        }
    }

}
