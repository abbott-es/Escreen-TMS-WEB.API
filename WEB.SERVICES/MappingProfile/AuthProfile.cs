using AutoMapper;
using WEB.DOMAIN.Entity;
using WEB.SERVICES.DTO;

namespace WEB.SERVICES.MappingProfile
{
    public class AuthProfile : Profile
    {
        public AuthProfile()
        {
            CreateMap<Auth, AuthDto>();
            CreateMap<AuthDto, Auth>()
                .ForMember(dest => dest.PasswordHash, opt => opt.MapFrom<PasswordEncryptionResolver>());
        }
    }
}
