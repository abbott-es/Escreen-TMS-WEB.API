using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using WEB.DOMAIN.Entity;

namespace WEB.DOMAIN.Config
{
    public class UserInfoConfig : IEntityTypeConfiguration<UserInfo>
    {
        public void Configure(EntityTypeBuilder<UserInfo> builder)
        {
            builder.HasKey(p => p.UserInfoID);

            builder.HasOne(ui => ui.User)
                   .WithOne(p => p.UserInfo)
                   .HasForeignKey<UserInfo>(ui => ui.UserInfoID)
                   .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
