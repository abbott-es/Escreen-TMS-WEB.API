using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using WEB.DOMAIN.Entity;

namespace WEB.DOMAIN.Config
{
    public class BookingStatusHistoryConfig : IEntityTypeConfiguration<BookingStatusHistory>
    {
        public void Configure(EntityTypeBuilder<BookingStatusHistory> builder)
        {
            builder.HasKey(sh => sh.StatusID);

            builder.Property(sh => sh.Status)
                   .IsRequired()
                   .HasMaxLength(50);

            builder.Property(sh => sh.ChangedDate)
                   .IsRequired();

            // Relationships
            builder.HasOne(sh => sh.Booking)
                   .WithMany(b => b.StatusHistory)
                   .HasForeignKey(sh => sh.BookingID)
                   .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(sh => sh.User)
                   .WithMany()
                   .HasForeignKey(sh => sh.ChangedByUser)
                   .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
