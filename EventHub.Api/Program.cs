using EventHub.Api.Data;
using EventHub.Api.Extensions;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddAllServices(builder.Configuration);

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    context.Database.Migrate();
    DbSeeder.Seed(context);
}
app.UseSwagger();
app.UseSwaggerUI();
app.UseMiddleware<EventHub.Api.Middleware.GlobalExceptionHandler>();
app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();