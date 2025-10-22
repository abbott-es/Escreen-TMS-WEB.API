using FluentValidation;
using FluentValidation.AspNetCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using WEB.DAL;
using WEB.DAL.AppDbContext;
using WEB.DAL.Repository;
using WEB.DOMAIN.Entity.Generic;
using WEB.DOMAIN.Interface;
using WEB.SERVICES.DTO.Generic;
using WEB.SERVICES.IService.IAuthentication;
using WEB.SERVICES.IService.IGeneric;
using WEB.SERVICES.MappingProfile.Authentication;
using WEB.SERVICES.MappingProfile.Generic;
using WEB.SERVICES.Service.Authentication;
using WEB.SERVICES.Service.Authentication.JWT;
using WEB.SERVICES.Service.Generic;
using WEB.SERVICES.Validation.Authentication;
using WEB.SERVICES.Validation.Generic;
using WEB.UTILITY.Logger;
using WEB.UTILITY.Security;
using WEB.UTILITY.Security.ISecurity;

namespace WEB.SERVICES
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddSharedServices(this IServiceCollection services, ConfigurationManager configuration)
        {
            services.AddAutoMapper(cfg =>
            {
                cfg.AddProfile<AuthProfile>();
                cfg.AddProfile<UserProfile>();
                cfg.AddProfile<RoleProfile>();
                cfg.AddProfile<ClientProfile>();
                cfg.AddProfile<VehicleProfile>();
                cfg.AddProfile<LocationProfile>();
            });
            services.AddTransient<IgnoreAuthInClientMapping>();

            services.AddMemoryCache();
            services.AddScoped<ITokenService, TokenService>();
            services.AddScoped<ITokenLifecycleService, TokenLifecycleService>();

            // Initialize the singleton manually
            var rsaKeyManager = RsaKeyManager.Instance;
            rsaKeyManager.LoadPublicKey(configuration["RsaKeys:Public"]);
            rsaKeyManager.LoadPrivateKey(configuration["RsaKeys:Private"]);
            services.AddSingleton(rsaKeyManager);
            services.AddHttpContextAccessor(); // Required for accessing HttpContext
            services.AddScoped<IUserContextService, UserContextService>();
            services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
            services.AddScoped(typeof(IBaseEntityRepository<>), typeof(BaseEntityRepository<>));
            services.AddScoped<IUnitOfWork, UnitOfWork>();
            services.AddSingleton(typeof(IAppLogger<>), typeof(AppLogger<>));
            services.AddScoped<IRsaEncryptionService, RsaEncryptionService>();
            services.AddScoped<PasswordEncryptionResolver>();
            services.AddTransient<GetSessionResolver>();
            services.AddTransient<UserWithoutAuthResolver>();
            services.AddScoped<IUserService, UserService>();//custom service
            services.AddScoped<IAuthService, AuthService>();
            services.AddScoped<IClientService, ClientService>();

            #region Control Tower Service
            services.AddScoped<IBookingService, BookingService>();


            #endregion

            return services;
        }

        //This keeps the shared project generic service
        public static IServiceCollection AddGenericService(this IServiceCollection services)
        {
            services.AddScoped(typeof(IGenericService<RoleDto>), typeof(GenericService<Role, RoleDto>));
            services.AddScoped(typeof(IGenericService<ClientDto>), typeof(GenericService<Client, ClientDto>));
            services.AddScoped(typeof(IGenericService<UserDto>), typeof(GenericService<User, UserDto>));
            services.AddScoped(typeof(IGenericService<LocationDto>), typeof(GenericService<Location, LocationDto>));
            services.AddScoped(typeof(IGenericService<VehicleDto>), typeof(GenericService<Vehicle, VehicleDto>));
            return services;
        }

        //This keeps the shared project generic and reusable across different database providers
        public static IServiceCollection AddDataAccess(this IServiceCollection services, Action<DbContextOptionsBuilder> optionsAction)
        {
            services.AddDbContext<WebApiDbContext>(optionsAction);
            return services;
        }

        public static IServiceCollection AddValidatorServices(this IServiceCollection services)
        {
            services.AddFluentValidationClientsideAdapters();
            services.AddValidatorsFromAssemblyContaining<AuthDTOValidator>();
            services.AddValidatorsFromAssemblyContaining<LogoutDtoValidator>();
            services.AddValidatorsFromAssemblyContaining<RoleDtoValidator>();
            services.AddScoped<IValidator<RoleDto>, RoleDtoValidator>();
            services.AddValidatorsFromAssemblyContaining<ClientDtoValidator>();
            services.AddScoped<IValidator<ClientDto>, ClientDtoValidator>();
            services.AddValidatorsFromAssemblyContaining<UserDtoValidator>();
            services.AddScoped<IValidator<UserDto>, UserDtoValidator>();
            services.AddValidatorsFromAssemblyContaining<LocationDtoValidator>();
            services.AddScoped<IValidator<LocationDto>, LocationDtoValidator>();
            services.AddValidatorsFromAssemblyContaining<VehicleDtoValidator>();
            services.AddScoped<IValidator<VehicleDto>, VehicleDtoValidator>();

            return services;
        }
    }
}
