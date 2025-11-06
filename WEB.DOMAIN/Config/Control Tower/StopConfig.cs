
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using WEB.DOMAIN.Entity;

namespace WEB.DOMAIN.Config
{
    public class StopConfig : IEntityTypeConfiguration<Stop>
    {
        public void Configure(EntityTypeBuilder<Stop> builder)
        {
            builder.HasKey(s => s.StopID);

            builder.HasOne(s => s.Coordinate)
                .WithMany()
                .HasForeignKey(x => x.CoordinateID)
                .IsRequired();
        }
    }
}
