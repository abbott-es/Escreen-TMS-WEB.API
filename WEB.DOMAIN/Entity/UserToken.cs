using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WEB.DOMAIN.Interface;

namespace WEB.DOMAIN.Entity
{
    public class UserToken : IEntity
    {
        public Guid TokenID { get; set; }
        public Guid UserID { get; set; }

        public string RefreshToken { get; set; }
        public DateTime RefreshTokenExpiry { get; set; }

        public string AccessTokenJti { get; set; }
        public DateTime IssuedAt { get; set; }
        public DateTime? RevokedAt { get; set; }
        public bool IsRevoked { get; set; }

        public string DeviceInfo { get; set; }

        public User User { get; set; }
    }
}
