using AutoMapper;
using WEB.DOMAIN.Entity.Generic;
using WEB.SERVICES.DTO.Generic;
using WEB.UTILITY.Enums;

namespace WEB.SERVICES.MappingProfile.Authentication
{
    public class GatewayProfile : Profile
    {
        public GatewayProfile()
        {
            CreateMap<GatewayDto, Gateway>()
                .ForMember(dest => dest.Method, opt => opt.MapFrom(src => (int)src.Method));
            CreateMap<Gateway, GatewayDto>()
                .ForMember(dest => dest.Method, opt => opt.MapFrom(src => Enum.GetName(typeof(Method), src.Method)));
        }
    }
}
