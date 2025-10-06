using AutoMapper;
using Isopoh.Cryptography.Argon2;
using WEB.DOMAIN.Entity;
using WEB.SERVICES.DTO;
using WEB.UTILITY.Security.ISecurity;

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
            string hashedPassword = Argon2.Hash(source.Password);
            return _rsaEncryptionService.Encrypt(hashedPassword);
        }
    }

}
