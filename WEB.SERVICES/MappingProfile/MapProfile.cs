using AutoMapper;
using WEB.DOMAIN.Entity;
using WEB.SERVICES.DTO;
namespace WEB.SERVICES.MappingProfile
{
    public class MapProfile : Profile
    {
        public MapProfile()
        {
            #region Auth
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
            #endregion

            #region Role
            CreateMap<RoleDto, Role>()
                .ForMember(dest => dest.RoleID, opt => opt.Ignore())
                .ForMember(dest => dest.RoleID, opt => opt.MapFrom(_ => Guid.NewGuid()));
            CreateMap<Role, RoleDto>();
            #endregion

            #region Client
            CreateMap<ClientDto, Client>()
                .ForMember(dest => dest.ClientID, opt => opt.MapFrom(_ => Guid.NewGuid()))
                .ForMember(dest => dest.User, opt => opt.MapFrom(src => src.User))
                .ForMember(dest => dest.Locations, opt => opt.MapFrom(src => src.Locations));
            CreateMap<UserDto, User>()
                .ForMember(dest => dest.RoleID, opt => opt.MapFrom(src => src.RoleID))
                .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(_ => DateTime.UtcNow))
                .ForMember(dest => dest.CreatedBy, opt => opt.MapFrom<GetSessionResolver>())
                .ForMember(dest => dest.Auth, opt => opt.Ignore());
            CreateMap<LocationDto, Location>();

            CreateMap<Client, ClientDto>()
                .ForMember(dest => dest.ClientID, opt => opt.MapFrom(src => src.ClientID))
                .ForMember(dest => dest.User, opt => opt.MapFrom(src => src.User))
                .ForMember(dest => dest.Locations, opt => opt.MapFrom(src => src.Locations));
            CreateMap<User, UserDto>()
                .ForMember(dest => dest.RoleID, opt => opt.MapFrom(src => src.RoleID))
                .ForMember(dest => dest.Auth, opt => opt.Ignore())
                .ForMember(dest => dest.UserInfo, opt => opt.MapFrom(src => src.UserInfo));
            CreateMap<UserInfo, UserInfoDto>();
            CreateMap<Location, LocationDto>();
            #endregion

        }
    }
}
