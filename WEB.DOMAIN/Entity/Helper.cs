using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WEB.DOMAIN.Entity
{
    public class Helper
    {
        public Guid UserID { get; set; }
        public Guid AssignedDriverID { get; set; }

        public UserInfo User { get; set; }
        public Driver AssignedDriver { get; set; }
    }

}
