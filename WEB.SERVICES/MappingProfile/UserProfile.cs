using AutoMapper;
using WEB.DOMAIN.Entity;
using WEB.SERVICES.DTO;

namespace WEB.SERVICES.MappingProfile
{
    public class UserProfile : Profile
    {
        public UserProfile()
        {
            CreateMap<UserDto, User>()
                .ForMember(dest => dest.UserID, opt => opt.MapFrom(src => src.UserID))
                .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(_ => DateTime.UtcNow))
                .ForMember(dest => dest.CreatedBy, opt => opt.MapFrom<GetSessionResolver>())
                .ForMember(dest => dest.IsActive, opt => opt.MapFrom(_ => true))
                .ForMember(dest => dest.RoleID, opt => opt.MapFrom(src => src.RoleID))
                .ForMember(dest => dest.Auth, opt => opt.MapFrom(src => src.Auth))
                .ForMember(dest => dest.UserInfo, opt => opt.MapFrom(src => src.UserInfo))
                .AfterMap<IgnoreAuthInClientMapping>();

            CreateMap<UserInfoDto, UserInfo>();
            CreateMap<UserInfo, UserInfoDto>();
            CreateMap<User, UserDto>()
                .ForMember(dest => dest.UserID, opt => opt.MapFrom(src => src.UserID))
                .ForMember(dest => dest.RoleID, opt => opt.MapFrom(src => src.RoleID))
                .ForMember(dest => dest.Auth, opt => opt.Ignore())
                .ForMember(dest => dest.UserInfo, opt => opt.MapFrom(src => src.UserInfo));

            CreateMap<UpdateUserDto, User>()
                .ForMember(dest => dest.UserInfo, opt => opt.MapFrom(src => src.UserInfo));
        }
    }
}
