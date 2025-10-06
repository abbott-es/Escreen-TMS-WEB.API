using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WEB.SERVICES.IService
{
    public interface IUserContextService
    {
        public string UserId { get; }
        public string Role { get; }
    }
}
