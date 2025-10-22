using AutoMapper;
using WEB.DOMAIN.Entity.Generic;
using WEB.SERVICES.DTO.Generic;

namespace WEB.SERVICES.MappingProfile.Generic
{
    public class LocationProfile : Profile
    {
        public LocationProfile()
        {
            CreateMap<LocationDto, Location>()
                .ForMember(dest => dest.LocationID, opt => opt.MapFrom(src => Guid.Empty == src.LocationID ? Guid.Empty : src.LocationID))
                .ForMember(dest => dest.ClientID, opt => opt.MapFrom(src => src.ClientID));
            CreateMap<Location, LocationDto>()
                .ForMember(dest => dest.LocationID, opt => opt.MapFrom(src => src.LocationID))
                .ForMember(dest => dest.ClientID, opt => opt.MapFrom(src => src.ClientID));
        }
    }
}
