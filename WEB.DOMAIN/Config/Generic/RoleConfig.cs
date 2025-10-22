using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using WEB.DOMAIN.Entity.Generic;

namespace WEB.DOMAIN.Config.Generic
{
    public class RoleConfig : IEntityTypeConfiguration<Role>
    {
        public void Configure(EntityTypeBuilder<Role> builder)
        {
            builder.HasKey(r => r.RoleID);

            builder.Property(r => r.RoleName)
                   .IsRequired()
                   .HasMaxLength(50);
        }
    }
}
