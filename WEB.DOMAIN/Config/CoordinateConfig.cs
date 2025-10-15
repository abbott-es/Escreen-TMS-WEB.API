using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using WEB.DOMAIN.Entity;

namespace WEB.DOMAIN.Config
{
    public class CoordinateConfig : IEntityTypeConfiguration<Coordinate>
    {
        public void Configure(EntityTypeBuilder<Coordinate> builder)
        {
            builder.HasKey(c => c.CoordinateID);

            builder.Property(c => c.CoordinateName)
                .HasMaxLength(100)
                .IsRequired(false);

            builder.Property(c => c.Latitude)
                .IsRequired();

            builder.Property(c => c.Longitude)
                .IsRequired();
        }
    }
}
