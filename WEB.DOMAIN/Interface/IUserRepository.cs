using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WEB.DOMAIN.Entity;

namespace WEB.DOMAIN.Interface
{
    public interface IUserRepository : IRepository<UserInfo>
    {
        Task<UserInfo?> GetByEmailAsync(string email);
    }
}
