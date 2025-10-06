using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WEB.DOMAIN.Entity
{
    public class TruckVendor
    {
        public Guid UserID { get; set; }
        public string CompanyName { get; set; }

        public UserInfo User { get; set; }
        public ICollection<TruckHead> SuppliedTrucks { get; set; }
    }

}
