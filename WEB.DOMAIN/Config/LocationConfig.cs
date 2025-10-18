using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using WEB.DOMAIN.Entity;

namespace WEB.DOMAIN.Config
{
    public class LocationConfig : IEntityTypeConfiguration<Location>
    {
        public void Configure(EntityTypeBuilder<Location> builder)
        {
            builder.HasKey(l => l.LocationID);

            builder.HasOne(l => l.Client)
                   .WithMany(c => c.Locations)
                   .HasForeignKey(l => l.ClientID)
                   .IsRequired(false);
        }
    }
}
