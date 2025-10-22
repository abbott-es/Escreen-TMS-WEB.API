using AutoMapper;
using WEB.DOMAIN.Entity.Generic;
using WEB.SERVICES.DTO.Generic;

namespace WEB.SERVICES.MappingProfile.Generic
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
