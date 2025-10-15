using AutoMapper;
using WEB.DOMAIN.Entity;
using WEB.SERVICES.DTO;
using WEB.SERVICES.IService;

namespace WEB.SERVICES.MappingProfile
{
    public class GetSessionResolver : IValueResolver<UserDto, User, string>
    {
        private readonly IUserContextService _userContextService;

        public GetSessionResolver(IUserContextService userContextService)
        {
            _userContextService = userContextService;
        }

        public string Resolve(UserDto source, User destination, string destMember, ResolutionContext context)
        {
            return _userContextService.UserId;
        }
    }
}
