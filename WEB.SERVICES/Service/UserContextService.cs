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

        public string UserId => _accessor.HttpContext?.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        public string Role => _accessor.HttpContext?.User?.FindFirst(ClaimTypes.Role)?.Value;
    }

}
