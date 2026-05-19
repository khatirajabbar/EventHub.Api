using EventHub.Api.Data;
using EventHub.Api.Mappings;
using EventHub.Api.Services;
using EventHub.Api.Validators;
using FluentValidation;
using FluentValidation.AspNetCore;
using Microsoft.EntityFrameworkCore;

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

        services.AddControllers();
        services.AddEndpointsApiExplorer();
        services.AddSwaggerGen();

        return services;
    }
}