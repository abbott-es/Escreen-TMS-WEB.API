using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using WEB.DOMAIN.Entity;

namespace WEB.DOMAIN.Config
{
    public class UserTokenConfiguration : IEntityTypeConfiguration<UserToken>
    {
        public void Configure(EntityTypeBuilder<UserToken> builder)
        {
            builder.HasKey(t => t.TokenID);

            builder.Property(t => t.RefreshToken)
                .IsRequired()
                .HasMaxLength(256);

            builder.Property(t => t.AccessTokenJti)
                .HasMaxLength(128);

            builder.Property(t => t.DeviceInfo)
                .HasMaxLength(256);

            builder.HasOne(t => t.User)
                .WithMany()
                .HasForeignKey(t => t.UserID)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }

}
