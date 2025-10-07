using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WEB.DOMAIN.Interface;

namespace WEB.DOMAIN.Entity
{
    public class Client : IEntity
    {
        public Guid ClientID { get; set; }
        public Guid UserID { get; set; }
        public string CompanyName { get; set; }

        public User User { get; set; }
        public ICollection<Location> Locations { get; set; }
    }

}
