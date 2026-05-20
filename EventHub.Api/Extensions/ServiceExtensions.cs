using EventHub.Api.Data;
using EventHub.Api.Mappings;
using EventHub.Api.Services;
using EventHub.Api.Settings;
using EventHub.Api.Validators;
using FluentValidation;
using FluentValidation.AspNetCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Text;

namespace EventHub.Api.Extensions;

public static class ServiceExtensions
{
    public static IServiceCollection AddAllServices(this IServiceCollection services, IConfiguration config)
    {
        services.AddDbContext<AppDbContext>(options =>
            options.UseSqlServer(config.GetConnectionString("DefaultConnection")));

        services.AddAutoMapper(typeof(MappingProfile));

        services.AddFluentValidationAutoValidation();
        services.AddValidatorsFromAssemblyContaining<EventCreateDtoValidator>();

        services.AddScoped<FileService>();
        services.AddScoped<IAuthService, AuthService>();

        // Configure JWT Settings
        var jwtSettings = config.GetSection("JwtSettings").Get<JwtSettings>();
        services.AddSingleton(jwtSettings);

        // Configure JWT Authentication
        services.AddAuthentication(options =>
        {
            options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
        })
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

        services.AddAuthorization();

        services.AddControllers();
        services.AddEndpointsApiExplorer();
        services.AddSwaggerGen();

        return services;
    }
}