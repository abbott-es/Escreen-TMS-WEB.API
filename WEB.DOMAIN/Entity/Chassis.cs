using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WEB.DOMAIN.Interface;

namespace WEB.DOMAIN.Entity
{
    public class Chassis : IEntity
    {
        public Guid ChassisID { get; set; }
        public string Type { get; set; }
        public string SerialNumber { get; set; }

        public Guid TruckHeadID { get; set; }

        public TruckHead TruckHead { get; set; }
    }

}
