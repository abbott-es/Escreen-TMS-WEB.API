using System.Reflection;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Serilog;
using StackExchange.Redis.Extensions.Core.Abstractions;
using StackExchange.Redis.Extensions.Core.Configuration;
using StackExchange.Redis.Extensions.System.Text.Json;
using System.Text;
using WEB.API.SwaggerFilter;
using WEB.SERVICES;
using WEB.UTILITY.Caching;
using WEB.SERVICES.Service.JWT;
using WEB.UTILITY.Convention;
using WEB.UTILITY.middleware;
using WEB.UTILITY.Security;

var builder = WebApplication.CreateBuilder(args);

// Read allowed origins from config
var allowedOrigins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>();

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowSpecificOrigins", policy =>
    {
        policy.WithOrigins(allowedOrigins)
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});

var jwtSettings = builder.Configuration.GetSection("JwtSettings").Get<JwtSettings>();
builder.Services.AddSingleton(jwtSettings);
// Add services to the container.


builder.Services.AddControllers(options =>
{
    options.Conventions.Add(new ApiResponseConvention());
});
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();
builder.Services.AddDataAccess(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));
builder.Services.AddSharedServices(builder.Configuration);
builder.Services.AddGenericService();
builder.Services.AddValidatorServices();
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwtSettings.Issuer,
            ValidAudience = jwtSettings.Audience,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings.SecretKey))
        };
    });

//builder.Services.AddAuthorization(options =>
//{
//    options.FallbackPolicy = new AuthorizationPolicyBuilder()
//        .RequireAuthenticatedUser()
//        .Build();
//});

builder.Services.AddScoped<ICache>(s => new SafeCache(
    new RedisCache(
        s.GetService<IRedisDatabase>(),
        s.GetService<IRedisClient>(),
        s.GetService<ILogger<RedisCache>>()),
    s.GetService<ILogger<SafeCache>>()));

var username = builder.Configuration["Redis:Username"];
var password = builder.Configuration["Redis:Password"];
var keyPrefix = builder.Configuration["Redis:KeyPrefix"];
builder.Services.AddStackExchangeRedisExtensions<SystemTextJsonSerializer>(new RedisConfiguration()
{
    Hosts = new RedisHost[]
        {
                new RedisHost
                {
                    Host = builder.Configuration["Redis:Host"],
                    Port = int.Parse(builder.Configuration["Redis:Port"])
                }
        },
    Ssl = bool.Parse(builder.Configuration["Redis:UseSsl"]),
    User = !string.IsNullOrEmpty(username) ? username : null,
    Password = !string.IsNullOrEmpty(password) ? password : null,
    KeyPrefix = !string.IsNullOrEmpty(keyPrefix) ? $"{keyPrefix}:" : string.Empty,
    SyncTimeout = int.Parse(builder.Configuration["Redis:SyncTimeout"])
});

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    var xmlFilename = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    options.IncludeXmlComments(Path.Combine(AppContext.BaseDirectory, xmlFilename));
    options.SchemaFilter<RandomGuidSchemaFilter>();
    options.AddSecurityDefinition("Bearer", new Microsoft.OpenApi.Models.OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = Microsoft.OpenApi.Models.SecuritySchemeType.Http,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = Microsoft.OpenApi.Models.ParameterLocation.Header,
        Description = "Enter your JWT token"
    });

    options.AddSecurityRequirement(new Microsoft.OpenApi.Models.OpenApiSecurityRequirement
    {
        {
            new Microsoft.OpenApi.Models.OpenApiSecurityScheme
            {
                Reference = new Microsoft.OpenApi.Models.OpenApiReference
                {
                    Type = Microsoft.OpenApi.Models.ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            new string[] {}
        }
    });
});

builder.Host.UseSerilog((ctx, lc) => lc
    .ReadFrom.Configuration(ctx.Configuration));
builder.Services.Configure<RouteOptions>(options =>
{
    options.LowercaseUrls = true;
});

var app = builder.Build();
app.UseCors("AllowSpecificOrigins");
app.UseSerilogRequestLogging();
// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.UseSwagger();
    app.UseSwaggerUI();
}
app.UseMiddleware<TraceIdInjectionMiddleware>();
app.UseHttpsRedirection();
app.UseMiddleware<TokenRevocationMiddleware>();
app.UseAuthentication();
app.UseMiddleware<JwtThrottlingMiddleware>();
app.UseAuthorization();

app.MapControllers();

app.Run();
