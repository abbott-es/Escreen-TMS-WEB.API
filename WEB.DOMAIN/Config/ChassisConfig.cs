using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using WEB.DOMAIN.Entity;

namespace WEB.DOMAIN.Config
{
    public class ChassisConfig : IEntityTypeConfiguration<Chassis>
    {
        public void Configure(EntityTypeBuilder<Chassis> builder)
        {
            builder.HasKey(c => c.ChassisID);

            builder.HasOne(c => c.TruckHead)
                   .WithOne(th => th.Chassis)
                   .HasPrincipalKey<TruckHead>(th => th.TruckHeadID)
                   .HasForeignKey<Chassis>(c => c.TruckHeadID)      
                   .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
