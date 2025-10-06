using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WEB.DOMAIN.Entity
{
    public class Auth
    {
        public Guid AuthID { get; set; }
        public Guid UserID { get; set; }
        public string Username { get; set; }
        public string PasswordHash { get; set; }
        public DateTime LastLogin { get; set; }

        public UserInfo User { get; set; }
    }
}
