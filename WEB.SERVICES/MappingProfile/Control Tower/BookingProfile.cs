using AutoMapper;
using WEB.DOMAIN.Entity;
using WEB.SERVICES.DTO;
using WEB.UTILITY.Enums;

namespace WEB.SERVICES.MappingProfile.Control_Tower
{
    public class BookingProfile : Profile
    {
        public BookingProfile()
        {
            CreateMap<Booking, BookingDto>();
            CreateMap<CreateBookingDto, Booking>()
                .ForMember(dest => dest.Status, opt => opt.MapFrom(_ => (int)BookingStatus.Pending))
                .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(_ => DateTime.UtcNow));

            CreateMap<Booking, BookingDetailDto>()
                .ForMember(dest => dest.DriverFullName, opt => opt.MapFrom(src => !string.IsNullOrEmpty(src.Driver.UserInfo.MiddleName) ? $"{src.Driver.UserInfo.FirstName} {src.Driver.UserInfo.MiddleName} {src.Driver.UserInfo.LastName}".Trim() : $"{src.Driver.UserInfo.FirstName} {src.Driver.UserInfo.LastName}".Trim()))
                .ForMember(dest => dest.HelperFullName, opt => opt.MapFrom(src => !string.IsNullOrEmpty(src.Helper.UserInfo.MiddleName) ? $"{src.Helper.UserInfo.FirstName} {src.Helper.UserInfo.MiddleName} {src.Helper.UserInfo.LastName}".Trim() : $"{src.Helper.UserInfo.FirstName} {src.Helper.UserInfo.LastName}".Trim()))
                .ForMember(dest => dest.StartRoute, opt => opt.MapFrom(src => src.StartRoute))
                .ForMember(dest => dest.EndRoute, opt => opt.MapFrom(src => src.EndRoute))
                .ForMember(dest => dest.StopRoute, opt => opt.MapFrom(src => src.StopRoute))
                .ForMember(dest => dest.Status, opt => opt.MapFrom(src => ((BookingStatus)src.Status).ToString()));
        }
    }
}
