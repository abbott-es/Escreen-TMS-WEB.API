using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WEB.DOMAIN.Interface;

namespace WEB.DOMAIN.Entity
{
    public class Auth : IEntity
    {
        public Guid AuthID { get; set; }
        public Guid UserID { get; set; }
        public string Username { get; set; }
        public string PasswordHash { get; set; }
        public DateTime LastLogin { get; set; }

        public User User { get; set; }
    }
}
