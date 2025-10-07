using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using WEB.DOMAIN.Entity;

namespace WEB.DOMAIN.Config
{
    public class TruckVendorConfig : IEntityTypeConfiguration<TruckVendor>
    {
        public void Configure(EntityTypeBuilder<TruckVendor> builder)
        {
            builder.HasKey(tv => tv.TruckVendorID);

            builder.HasOne(tv => tv.User)
                   .WithOne(u => u.TruckVendor)
                   .HasForeignKey<TruckVendor>(tv => tv.UserID);
        }
    }
}
