using AutoMapper;
using WEB.DOMAIN.Entity.Generic;
using WEB.SERVICES.DTO.Generic;

namespace WEB.SERVICES.MappingProfile.Generic
{
    public class UserWithoutAuthResolver : IValueResolver<ClientDto, Client, User>
    {
        private readonly IMapper _mapper;

        public UserWithoutAuthResolver(IMapper mapper)
        {
            _mapper = mapper;
        }

        public User Resolve(ClientDto source, Client destination, User destMember, ResolutionContext context)
        {
            return _mapper.Map<UserDto, User>(source.User, opts =>
            {
                opts.Items["IgnoreAuth"] = true;
            });
        }
    }
}
