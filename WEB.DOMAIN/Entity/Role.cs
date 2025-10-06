using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WEB.DOMAIN.Entity
{
    public class Role
    {
        public Guid RoleID { get; set; }
        public string RoleName { get; set; }

        public ICollection<UserInfo> Users { get; set; }
    }

}
