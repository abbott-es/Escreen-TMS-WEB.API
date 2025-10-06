using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WEB.DOMAIN.Entity
{
    public class Chassis
    {
        public Guid ChassisID { get; set; }
        public string Type { get; set; }
        public string SerialNumber { get; set; }
        public Guid TruckID { get; set; }

        public TruckHead TruckHead { get; set; }
    }

}
