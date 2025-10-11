using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using WEB.DOMAIN.Entity;

namespace WEB.DOMAIN.Config
{
    public class BookingConfig : IEntityTypeConfiguration<Booking>
    {
        public void Configure(EntityTypeBuilder<Booking> builder)
        {
            builder.HasKey(b => b.BookingID);

            builder.Property(b => b.Status)
                   .IsRequired()
                   .HasMaxLength(50);

            builder.Property(b => b.ScheduledDate)
                   .IsRequired();

            // Relationships                                           
            builder.HasOne(b => b.Client)
                   .WithMany()
                   .HasForeignKey(b => b.ClientID)
                   .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(b => b.Location)
                   .WithMany()
                   .HasForeignKey(b => b.LocationID)
                   .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(b => b.TruckHead)
                   .WithMany()
                   .HasForeignKey(b => b.TruckID)
                   .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(b => b.Driver)
                   .WithMany()
                   .HasForeignKey(b => b.DriverID)
                   .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(b => b.Helper)
                   .WithMany()
                   .HasForeignKey(b => b.HelperID)
                   .OnDelete(DeleteBehavior.SetNull);

            builder.HasMany(b => b.StatusHistory)
                   .WithOne(sh => sh.Booking)
                   .HasForeignKey(sh => sh.BookingID)
                   .OnDelete(DeleteBehavior.Cascade);
        }
    }
}