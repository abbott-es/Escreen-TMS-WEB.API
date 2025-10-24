using AutoMapper;
using WEB.DOMAIN.Entity;
using WEB.SERVICES.DTO;

namespace WEB.SERVICES.MappingProfile.Control_Tower
{
    public class StopProfile : Profile
    {
        public StopProfile()
        {
            CreateMap<Stop, StopDto>();
            CreateMap<StopDto, Stop>();
        }
    }
}
