using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WEB.DOMAIN.Entity;

namespace WEB.DOMAIN.Config
{
    public class DriverConfig : IEntityTypeConfiguration<Driver>
    {
        public void Configure(EntityTypeBuilder<Driver> builder)
        {
            builder.HasKey(d => d.DriverID);

            builder.HasOne(d => d.User)
                   .WithOne(u => u.Driver)
                   .HasForeignKey<Driver>(d => d.UserID)
                   .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(d => d.AssignedTruck)
                   .WithMany()
                   .HasForeignKey(d => d.AssignedTruckID)
                   .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
