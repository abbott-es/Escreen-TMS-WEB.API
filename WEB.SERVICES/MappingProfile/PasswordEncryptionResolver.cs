using AutoMapper;
using WEB.DOMAIN.Entity;
using WEB.SERVICES.DTO;
using WEB.UTILITY.Security;

namespace WEB.SERVICES.MappingProfile
{
    public class PasswordEncryptionResolver : IValueResolver<UserDto, Auth, string>
    {
        private readonly IRsaEncryptionService _rsaEncryptionService;

        public PasswordEncryptionResolver(IRsaEncryptionService rsaEncryptionService)
        {
            _rsaEncryptionService = rsaEncryptionService;
        }

        public string Resolve(UserDto source, Auth destination, string destMember, ResolutionContext context)
        {
            return _rsaEncryptionService.Encrypt(source.Password);
        }
    }

}
