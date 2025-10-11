using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using WEB.DOMAIN.Entity;

namespace WEB.DOMAIN.Config
{
    public class TruckHeadConfig : IEntityTypeConfiguration<TruckHead>
    {
        public void Configure(EntityTypeBuilder<TruckHead> builder)
        {
            builder.HasKey(t => t.TruckHeadID);

            builder.HasOne(t => t.Vendor)
                   .WithMany(v => v.SuppliedTrucks)
                   .HasForeignKey(t => t.VendorID);

        }
    }
}
