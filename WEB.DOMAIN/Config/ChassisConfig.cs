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

            builder.HasOne(c => c.Vehicle)
                   .WithOne(th => th.Chassis)
                   .HasPrincipalKey<Vehicle>(th => th.VehicleID)
                   .HasForeignKey<Chassis>(c => c.VehicleID)      
                   .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
