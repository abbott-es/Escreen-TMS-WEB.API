using FluentValidation;
using FluentValidation.AspNetCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using WEB.DAL;
using WEB.DAL.AppDbContext;
using WEB.DAL.Repository;
using WEB.DOMAIN.Interface;
using WEB.SERVICES.DTO;
using WEB.SERVICES.IService;
using WEB.SERVICES.MappingProfile;
using WEB.SERVICES.Service;
using WEB.SERVICES.Service.JWT;
using WEB.SERVICES.Validation;
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
                cfg.AddProfile<MapProfile>();
            });

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
            services.AddScoped<IUnitOfWork, UnitOfWork>();
            services.AddScoped<IUserRepository, UserRepository>();
            services.AddSingleton(typeof(IAppLogger<>), typeof(AppLogger<>));
            services.AddScoped<IRsaEncryptionService, RsaEncryptionService>();
            services.AddScoped<PasswordEncryptionResolver>();
            services.AddTransient<GetSessionResolver>();
            services.AddScoped<IGenericService<UserDto>, UserService>();//for generic service
            services.AddScoped<IUserService, UserService>();//custom service
            services.AddScoped<IAuthService, AuthService>();

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

            return services;
        }
    }
}
