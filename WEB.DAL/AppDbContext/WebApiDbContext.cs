using Microsoft.EntityFrameworkCore;
using WEB.DOMAIN.Entity;

namespace WEB.DAL.AppDbContext
{
    public class WebApiDbContext : DbContext
    {
        //run this command from the root folder
        //dotnet ef migrations add initialDB --project WEB.DAL --startup-project WEB.API
        public WebApiDbContext(DbContextOptions<WebApiDbContext> options)
            : base(options)
        {
        }

        // Core Entities
        public DbSet<UserInfo> Users { get; set; }
        public DbSet<Auth> Auths { get; set; }
        public DbSet<Role> Roles { get; set; }

        // Specialized User Roles
        public DbSet<Driver> Drivers { get; set; }
        public DbSet<Client> Clients { get; set; }
        public DbSet<Helper> Helpers { get; set; }
        public DbSet<TruckVendor> TruckVendors { get; set; }

        // Vehicle Entities
        public DbSet<TruckHead> TruckHeads { get; set; }
        public DbSet<Chassis> Chassis { get; set; }

        // Location
        public DbSet<Location> Locations { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            foreach (var entityType in modelBuilder.Model.GetEntityTypes())
            {
                if (typeof(BaseEntity).IsAssignableFrom(entityType.ClrType))
                {
                    modelBuilder.Entity(entityType.ClrType).Property(nameof(BaseEntity.IsActive)).HasDefaultValue(true);
                    modelBuilder.Entity(entityType.ClrType).Property(nameof(BaseEntity.CreatedAt)).HasDefaultValueSql("GETUTCDATE()");
                }
            }

            modelBuilder.Entity<Auth>()
            .HasKey(a => a.AuthID);

            modelBuilder.Entity<Auth>()
                        .HasOne(a => a.User)
                        .WithOne(u => u.Auth)
                        .HasForeignKey<Auth>(a => a.UserID);

            modelBuilder.Entity<UserInfo>()
                        .HasKey(u => u.UserID);
            
            modelBuilder.Entity<UserInfo>()
                        .HasOne(u => u.Role)
                        .WithMany(r => r.Users)
                        .HasForeignKey(u => u.RoleID);

            modelBuilder.Entity<Role>()
                        .HasKey(r => r.RoleID);

            modelBuilder.Entity<Role>()
                        .Property(r => r.RoleName)
                        .IsRequired()
                        .HasMaxLength(50);

            modelBuilder.Entity<Driver>()
                        .HasKey(d => d.UserID);

            modelBuilder.Entity<Driver>()
                        .HasOne(d => d.User)
                        .WithOne(u => u.Driver)
                        .HasForeignKey<Driver>(d => d.UserID);

            modelBuilder.Entity<Driver>()
                        .HasOne(d => d.AssignedTruck)
                        .WithMany()
                        .HasForeignKey(d => d.AssignedTruckID);

            modelBuilder.Entity<Client>()
                        .HasKey(c => c.UserID);

            modelBuilder.Entity<Client>()
                        .HasOne(c => c.User)
                        .WithOne(u => u.Client)
                        .HasForeignKey<Client>(c => c.UserID);

            modelBuilder.Entity<Helper>()
                        .HasKey(h => h.UserID);

            modelBuilder.Entity<Helper>()
                        .HasOne(h => h.User)
                        .WithOne(u => u.Helper)
                        .HasForeignKey<Helper>(h => h.UserID)
                        .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Helper>()
                        .HasOne(h => h.AssignedDriver)
                        .WithMany(d => d.Helpers)
                        .HasForeignKey(h => h.AssignedDriverID)
                        .OnDelete(DeleteBehavior.Cascade);;

            modelBuilder.Entity<TruckVendor>()
                        .HasKey(tv => tv.UserID);

            modelBuilder.Entity<TruckVendor>()
                        .HasOne(tv => tv.User)
                        .WithOne(u => u.TruckVendor)
                        .HasForeignKey<TruckVendor>(tv => tv.UserID);

            modelBuilder.Entity<TruckHead>()
                        .HasKey(t => t.TruckID);

            modelBuilder.Entity<TruckHead>()
                        .HasOne(t => t.Vendor)
                        .WithMany(v => v.SuppliedTrucks)
                        .HasForeignKey(t => t.VendorID);

            modelBuilder.Entity<Chassis>()
                        .HasKey(c => c.ChassisID);

            modelBuilder.Entity<Chassis>()
                        .HasOne(c => c.TruckHead)
                        .WithOne(t => t.Chassis)
                        .HasForeignKey<Chassis>(c => c.TruckID);
            modelBuilder.Entity<Location>()
                        .HasKey(l => l.LocationID);

            modelBuilder.Entity<Location>()
                        .HasOne(l => l.Client)
                        .WithMany(c => c.Locations)
                        .HasForeignKey(l => l.ClientID);

            modelBuilder.Entity<Role>().HasData(
                new Role
                {
                    RoleID = Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa"),
                    RoleName = "Super Admin"
                },
                new Role
                {
                    RoleID = Guid.Parse("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb"),
                    RoleName = "Admin"
                }
            );

        }

    }
}
