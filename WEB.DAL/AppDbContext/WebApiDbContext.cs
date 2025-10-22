using Microsoft.EntityFrameworkCore;
using System.Reflection;
using WEB.DOMAIN.Config.Generic;
using WEB.DOMAIN.Entity;
using WEB.DOMAIN.Entity.Generic;
using WEB.DOMAIN.Interface;

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

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            #region Auto Register Entities
            var entityTypes = Assembly.GetExecutingAssembly()
                .GetTypes()
                .Where(t => typeof(IEntity).IsAssignableFrom(t) && t.IsClass && !t.IsAbstract);

            foreach (var type in entityTypes)
            {
                modelBuilder.Entity(type);
            }
            #endregion
            #region Auto Apply Configs
            var configAssembly = typeof(UserConfig).Assembly;

            var applyConfigMethod = typeof(ModelBuilder)
                .GetMethods()
                .First(m => m.Name == nameof(ModelBuilder.ApplyConfiguration));

            var configTypes = configAssembly
                .GetTypes()
                .Where(t => t.IsClass && !t.IsAbstract)
                .Select(t => new
                {
                    Type = t,
                    Interface = t.GetInterfaces()
                                 .FirstOrDefault(i => i.IsGenericType &&
                                                      i.GetGenericTypeDefinition() == typeof(IEntityTypeConfiguration<>))
                })
                .Where(x => x.Interface != null);

            foreach (var config in configTypes)
            {
                var entityType = config.Interface.GetGenericArguments()[0];
                var method = applyConfigMethod.MakeGenericMethod(entityType);
                method.Invoke(modelBuilder, new[] { Activator.CreateInstance(config.Type) });
            }
            #endregion
            base.OnModelCreating(modelBuilder);
            #region Default Value
            foreach (var entityType in modelBuilder.Model.GetEntityTypes())
            {
                if (typeof(BaseEntity).IsAssignableFrom(entityType.ClrType))
                {
                    modelBuilder.Entity(entityType.ClrType).Property(nameof(BaseEntity.IsActive)).HasDefaultValue(true);
                    modelBuilder.Entity(entityType.ClrType).Property(nameof(BaseEntity.CreatedAt)).HasDefaultValueSql("GETUTCDATE()");
                }
            }

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
                },
                new Role
                {
                    RoleID = Guid.Parse("cccccccc-cccc-cccc-cccc-cccccccccccc"),
                    RoleName = "Driver"
                },
                new Role
                {
                    RoleID = Guid.Parse("dddddddd-dddd-dddd-dddd-dddddddddddd"),
                    RoleName = "Helper"
                },
                new Role
                {
                    RoleID = Guid.Parse("eeeeeeee-eeee-eeee-eeee-eeeeeeeeeeee"),
                    RoleName = "Dispatcher"
                }
            );
            #endregion
        }
    }
}
