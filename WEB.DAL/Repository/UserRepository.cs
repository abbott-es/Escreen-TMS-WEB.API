using Microsoft.EntityFrameworkCore;
using WEB.DOMAIN.Entity;
using WEB.DOMAIN.Interface;

namespace WEB.DAL.Repository
{
    public class UserRepository : Repository<UserInfo>, IUserRepository
    {
        public UserRepository(AppDbContext.WebApiDbContext context) : base(context) { }

        public async Task<UserInfo?> GetByEmailAsync(string email)
        {
            return await _dbSet.FirstOrDefaultAsync(u => u.Email == email);
        }
    }
}
