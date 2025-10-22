using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using WEB.DOMAIN.Entity.Generic;

namespace WEB.DOMAIN.Config.Generic
{
    public class UserInfoConfig : IEntityTypeConfiguration<UserInfo>
    {
        public void Configure(EntityTypeBuilder<UserInfo> builder)
        {
            builder.HasKey(p => p.UserInfoID);

            builder.HasOne(ui => ui.User)
                   .WithOne(p => p.UserInfo)
                   .HasForeignKey<UserInfo>(ui => ui.UserID)
                   .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
