using AutoMapper;
using WEB.DOMAIN.Entity.Generic;
using WEB.SERVICES.DTO.Generic;

namespace WEB.SERVICES.MappingProfile.Generic
{
    public class ClientProfile : Profile
    {
        public ClientProfile()
        {
            CreateMap<ClientDto, Client>()
                .ForMember(dest => dest.ClientID, opt => opt.Ignore())
                .ForMember(dest => dest.User, opt => opt.MapFrom<UserWithoutAuthResolver>())
                .ForMember(dest => dest.Locations, opt => opt.MapFrom(src => src.Locations));

            CreateMap<Client, ClientDto>()
                .ForMember(dest => dest.ClientID, opt => opt.MapFrom(src => src.ClientID))
                .ForMember(dest => dest.User, opt => opt.MapFrom(src => src.User))
                .ForMember(dest => dest.Locations, opt => opt.MapFrom(src => src.Locations));
        }
    }
}
