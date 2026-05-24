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
using Microsoft.OpenApi.Models;
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
        services.AddScoped<IEmailService, EmailService>();

        // Configure JWT Settings
        var jwtSettings = config.GetSection("JwtSettings").Get<JwtSettings>();
        if (jwtSettings == null)
            throw new InvalidOperationException("JwtSettings configuration is missing from appsettings.json");
        services.AddSingleton(jwtSettings);

        // Configure Email Settings
        var emailSettings = config.GetSection("EmailSettings").Get<EmailSettings>();
        if (emailSettings == null)
            throw new InvalidOperationException("EmailSettings configuration is missing from appsettings.json");
        services.AddSingleton(emailSettings);

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

        services.AddControllers()
            .AddJsonOptions(options =>
            {
                // Configure JSON to use enum names instead of numbers
                options.JsonSerializerOptions.Converters.Add(new System.Text.Json.Serialization.JsonStringEnumConverter());
            });
        services.AddEndpointsApiExplorer();
        services.AddSwaggerGen(options =>
        {
            // Add JWT Bearer Authentication to Swagger
            var securityScheme = new OpenApiSecurityScheme
            {
                Name = "JWT Authentication",
                Description = "Enter JWT token",
                In = ParameterLocation.Header,
                Type = SecuritySchemeType.Http,
                Scheme = "bearer",
                BearerFormat = "JWT"
            };

            options.AddSecurityDefinition("Bearer", securityScheme);

            var securityRequirement = new OpenApiSecurityRequirement
            {
                {
                    new OpenApiSecurityScheme
                    {
                        Reference = new OpenApiReference
                        {
                            Type = ReferenceType.SecurityScheme,
                            Id = "Bearer"
                        }
                    },
                    new string[] { }
                }
            };

            options.AddSecurityRequirement(securityRequirement);
            
            // Configure Swagger to show enum values as strings
            options.SchemaFilter<EnumSchemaFilter>();
            options.ParameterFilter<EnumParameterFilter>();
            options.OperationFilter<EnumParameterFilter>();
        });

        return services;
    }
}