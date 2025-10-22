using AutoMapper;
using WEB.DOMAIN.Entity;
using WEB.SERVICES.DTO;

namespace WEB.SERVICES.MappingProfile
{
    public class RoleProfile : Profile
    {
        public RoleProfile()
        {
            CreateMap<RoleDto, Role>()
            .ForMember(dest => dest.RoleID, opt => opt.MapFrom(src => Guid.Empty == src.RoleId ? Guid.Empty : src.RoleId))
            .ForMember(dest => dest.IsActive, opt => opt.MapFrom(_ => true));
            CreateMap<Role, RoleDto>();
        }
    }
}
