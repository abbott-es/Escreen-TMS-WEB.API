using AutoMapper;
using WEB.DOMAIN.Entity;
using WEB.SERVICES.DTO;
using WEB.SERVICES.MappingProfile.Generic;
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
        }
    }
}
