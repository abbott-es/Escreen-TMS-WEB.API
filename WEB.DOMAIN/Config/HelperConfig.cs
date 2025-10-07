using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using WEB.DOMAIN.Entity;

namespace WEB.DOMAIN.Config
{
    public class HelperConfig : IEntityTypeConfiguration<Helper>
    {
        public void Configure(EntityTypeBuilder<Helper> builder)
        {
            builder.HasKey(h => h.HelperID);

            builder.HasOne(h => h.User)
                   .WithOne(u => u.Helper)
                   .HasForeignKey<Helper>(h => h.UserID)
                   .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(h => h.AssignedDriver)
                   .WithMany(d => d.Helpers)
                   .HasForeignKey(h => h.AssignedDriverID)
                   .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
