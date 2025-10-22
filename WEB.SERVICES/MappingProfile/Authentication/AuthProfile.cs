using AutoMapper;
using WEB.DOMAIN.Entity.Authentication;
using WEB.SERVICES.DTO.Authentication;

namespace WEB.SERVICES.MappingProfile.Authentication
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
