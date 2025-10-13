using AutoMapper;
using WEB.DOMAIN.Entity;
using WEB.SERVICES.DTO;

namespace WEB.SERVICES.MappingProfile
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
