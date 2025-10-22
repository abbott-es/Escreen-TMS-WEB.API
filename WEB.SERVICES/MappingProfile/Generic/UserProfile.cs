using AutoMapper;
using WEB.DOMAIN.Entity.Generic;
using WEB.SERVICES.DTO.Generic;

namespace WEB.SERVICES.MappingProfile.Generic
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

            CreateMap<UserInfoDto, UserInfo>()
                .ForMember(dest => dest.UserInfoID,
                    opt => opt.MapFrom(src => Guid.Empty == src.UserInfoID ? Guid.Empty : src.UserInfoID))
                .ForMember(dest => dest.MiddleName,
                    opt => opt.MapFrom(src => string.IsNullOrEmpty(src.MiddleName) ? string.Empty : src.MiddleName));

            CreateMap<UserInfo, UserInfoDto>();

            CreateMap<User, UserDto>()
                .ForMember(dest => dest.UserID, opt => opt.MapFrom(src => src.UserID))
                .ForMember(dest => dest.RoleID, opt => opt.MapFrom(src => src.RoleID))
                .ForMember(dest => dest.Auth, opt => opt.Ignore())
                .ForMember(dest => dest.UserInfo, opt => opt.MapFrom(src => src.UserInfo));

            CreateMap<UserInfo, DriverHelperDto>()
                .ForMember(dest => dest.FullName, opt => opt.MapFrom(src => !string.IsNullOrEmpty(src.MiddleName) ? $"{src.FirstName} {src.MiddleName} {src.LastName}".Trim() : $"{src.FirstName} {src.LastName}".Trim()));
        }

        private string SplitName(string fullName, string type)
        {
            if (string.IsNullOrWhiteSpace(fullName) || string.IsNullOrWhiteSpace(type))
                return string.Empty;

            var split = fullName.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            if (split.Length == 0)
                return string.Empty;

            return type.ToLower() switch
            {
                "first" => split[0],
                "last" => split.Length >= 3 ? split[2] : split.Length >= 2 ? split[1] : string.Empty,
                _ => split.Length >= 2 ? split[1] : string.Empty
            };
        }

    }
}
