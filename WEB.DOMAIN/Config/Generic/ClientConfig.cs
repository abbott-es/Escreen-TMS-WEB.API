using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using WEB.DOMAIN.Entity.Generic;

namespace WEB.DOMAIN.Config.Generic
{
    public class ClientConfig : IEntityTypeConfiguration<Client>
    {
        public void Configure(EntityTypeBuilder<Client> builder)
        {
            builder.HasKey(c => c.ClientID);

            builder.HasOne(c => c.User)
                   .WithOne(u => u.Client)
                   .HasForeignKey<Client>(c => c.UserID)
                   .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
