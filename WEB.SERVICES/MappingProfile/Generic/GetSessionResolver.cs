using AutoMapper;
using WEB.DOMAIN.Entity.Generic;
using WEB.SERVICES.DTO.Generic;
using WEB.SERVICES.IService.IGeneric;

namespace WEB.SERVICES.MappingProfile.Generic
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
