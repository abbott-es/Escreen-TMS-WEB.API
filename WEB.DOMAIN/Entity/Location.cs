using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WEB.DOMAIN.Entity
{
    public class Location
    {
        public Guid LocationID { get; set; }
        public string Name { get; set; }
        public string Address { get; set; }
        public string Type { get; set; }

        public Guid? ClientID { get; set; }
        public Client Client { get; set; }
    }

}
