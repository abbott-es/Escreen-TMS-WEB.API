
namespace WEB.SERVICES.MappingProfile
{
    using AutoMapper;
    using WEB.DOMAIN.Entity;
    using WEB.SERVICES.DTO;

    public class MapProfile : Profile
    {
        public MapProfile()
        {
            CreateMap<UserDto, User>()
                .ForMember(dest => dest.UserID, opt => opt.MapFrom(_ => Guid.NewGuid()))
                .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(_ => DateTime.UtcNow))
                .ForMember(dest => dest.CreatedBy, opt => opt.MapFrom<GetSessionResolver>())
                .ForMember(dest => dest.IsActive, opt => opt.MapFrom(_ => true))
                .ForMember(dest => dest.RoleID, opt => opt.MapFrom(src => src.RoleID))
                .ForMember(dest => dest.Auth, opt => opt.MapFrom(src => src.Auth))
                .ForMember(dest => dest.UserInfo, opt => opt.MapFrom(src => src.UserInfo));
            CreateMap<UserInfoDto, UserInfo>();
            CreateMap<AuthDto, Auth>()
                .ForMember(dest => dest.PasswordHash, opt => opt.MapFrom<PasswordEncryptionResolver>());
        }
    }
}
