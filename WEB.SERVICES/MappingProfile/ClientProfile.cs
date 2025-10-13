using AutoMapper;
using WEB.DOMAIN.Entity;
using WEB.SERVICES.DTO;

namespace WEB.SERVICES.MappingProfile
{
    public class ClientProfile : Profile
    {
        public ClientProfile()
        {
            CreateMap<ClientDto, Client>()
                .ForMember(dest => dest.ClientID, opt => opt.MapFrom(_ => Guid.NewGuid()))
                .ForMember(dest => dest.User, opt => opt.MapFrom<UserWithoutAuthResolver>())
                .ForMember(dest => dest.Locations, opt => opt.MapFrom(src => src.Locations));

            CreateMap<LocationDto, Location>();
            CreateMap<Location, LocationDto>();
            CreateMap<Client, ClientDto>()
                .ForMember(dest => dest.ClientID, opt => opt.MapFrom(src => src.ClientID))
                .ForMember(dest => dest.User, opt => opt.MapFrom(src => src.User))
                .ForMember(dest => dest.Locations, opt => opt.MapFrom(src => src.Locations));
        }
    }
}
