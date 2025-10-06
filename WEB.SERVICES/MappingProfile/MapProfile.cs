
namespace WEB.SERVICES.MappingProfile
{
    using AutoMapper;
    using WEB.DOMAIN.Entity;
    using WEB.SERVICES.DTO;

    public class MapProfile : Profile
    {
        public MapProfile()
        {
            CreateMap<UserDto, UserInfo>()
                .ForMember(dest => dest.UserID, opt => opt.MapFrom(_ => Guid.NewGuid()))
                .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(_ => DateTime.UtcNow))
                .ForMember(dest => dest.IsActive, opt => opt.MapFrom(_ => true));

            CreateMap<UserDto, Auth>()
                .ForMember(dest => dest.AuthID, opt => opt.MapFrom(_ => Guid.NewGuid()))
                .ForMember(dest => dest.UserID, opt => opt.Ignore())
                .ForMember(dest => dest.PasswordHash, opt => opt.MapFrom<PasswordEncryptionResolver>());

        }
    }
}
