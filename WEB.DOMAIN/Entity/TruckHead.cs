using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WEB.DOMAIN.Interface;

namespace WEB.DOMAIN.Entity
{
    public class TruckHead : IEntity
    {
        public Guid TruckHeadID { get; set; }
        public string Model { get; set; }
        public string PlateNumber { get; set; }
        public Guid VendorID { get; set; }

        public TruckVendor Vendor { get; set; }
        public Chassis Chassis { get; set; }
    }

}
