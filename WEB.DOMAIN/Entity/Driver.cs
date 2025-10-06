using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WEB.DOMAIN.Entity
{
    public class Driver
    {
        public Guid UserID { get; set; }
        public string LicenseNumber { get; set; }
        public Guid? AssignedTruckID { get; set; }

        public UserInfo User { get; set; }
        public TruckHead AssignedTruck { get; set; }
        public ICollection<Helper> Helpers { get; set; }
    }

}
