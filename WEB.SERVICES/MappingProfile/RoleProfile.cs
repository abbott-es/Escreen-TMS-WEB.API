using AutoMapper;
using WEB.DOMAIN.Entity;
using WEB.SERVICES.DTO;

namespace WEB.SERVICES.MappingProfile
{
    public class RoleProfile : Profile
    {
        public RoleProfile() {

            CreateMap<RoleDto, Role>()
            .ForMember(dest => dest.RoleID, opt => opt.Ignore())
            .ForMember(dest => dest.RoleID, opt => opt.MapFrom(_ => Guid.NewGuid()));
            CreateMap<Role, RoleDto>();
        }
    }
}
