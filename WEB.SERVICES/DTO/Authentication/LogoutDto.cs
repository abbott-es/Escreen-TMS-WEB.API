using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WEB.SERVICES.DTO.Authentication
{
    public class LogoutDto
    {
        public string AccessToken { get; set; }
        public string RefreshToken { get; set; }
    }
}
