using AutoMapper;
using WEB.DOMAIN.Entity;
using WEB.SERVICES.DTO;

namespace WEB.SERVICES.MappingProfile
{
    public class VehicleProfile : Profile
    {
        public VehicleProfile()
        {
            CreateMap<Vehicle, VehicleDto>()
                .ForMember(dest => dest.Chassis, opt => opt.MapFrom(src => src.Chassis));

            CreateMap<Chassis, ChassisDto>();

            CreateMap<VehicleDto, Vehicle>()
                .ForMember(dest => dest.VehicleID, opt => opt.MapFrom(src => Guid.Empty == src.VehicleID ? Guid.Empty : src.VehicleID))
                .ForMember(dest => dest.Chassis, opt => opt.MapFrom(src => src.Chassis));

            CreateMap<ChassisDto, Chassis>()
                .ForMember(dest => dest.ChassisID, opt => opt.MapFrom(src => Guid.Empty == src.ChassisID ? Guid.Empty : src.ChassisID));
        }
    }
}
