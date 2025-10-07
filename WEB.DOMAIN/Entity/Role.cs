using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WEB.DOMAIN.Interface;

namespace WEB.DOMAIN.Entity
{
    public class Role : IEntity
    {
        public Guid RoleID { get; set; }
        public string RoleName { get; set; }

        public ICollection<User> Users { get; set; }
    }

}
