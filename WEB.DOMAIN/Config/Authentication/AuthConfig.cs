using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using WEB.DOMAIN.Entity.Authentication;

namespace WEB.DOMAIN.Config.Authentication
{
    public class AuthConfig : IEntityTypeConfiguration<Auth>
    {
        public void Configure(EntityTypeBuilder<Auth> builder)
        {
            builder.HasKey(a => a.AuthID);

            builder.HasOne(a => a.User)
                   .WithOne(u => u.Auth)
                   .HasForeignKey<Auth>(a => a.UserID);
        }
    }
}
