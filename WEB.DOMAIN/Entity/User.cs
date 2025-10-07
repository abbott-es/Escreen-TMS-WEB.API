using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WEB.DOMAIN.Interface;

namespace WEB.DOMAIN.Entity
{
    public class User : BaseEntity, IEntity
    {
        public Guid UserID { get; set; }
        public Guid RoleID { get; set; }
        public Role Role { get; set; }

        public Auth Auth { get; set; }
        public Driver Driver { get; set; }
        public Client Client { get; set; }
        public Helper Helper { get; set; }
        public TruckVendor TruckVendor { get; set; }
        public virtual UserInfo UserInfo { get; set; }
    }
}
