using AutoMapper;
using WEB.DOMAIN.Entity;
using WEB.SERVICES.DTO;

namespace WEB.SERVICES.MappingProfile.Control_Tower
{
    public class CoordinateProfile : Profile
    {
        public CoordinateProfile()
        {
            CreateMap<Coordinate, CoordinateDto>();
            CreateMap<CoordinateDto, Coordinate>();
        }
    }
}
