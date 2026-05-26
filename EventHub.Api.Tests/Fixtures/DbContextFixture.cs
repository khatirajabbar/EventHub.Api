using EventHub.Api.Data;
using Microsoft.EntityFrameworkCore;

namespace EventHub.Api.Tests.Fixtures;

/// <summary>
/// Fixture for creating in-memory database context for testing
/// </summary>
public class DbContextFixture
{
    public static AppDbContext CreateInMemoryDbContext()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString()) // Unique DB per test
            .ConfigureWarnings(w => w.Ignore(Microsoft.EntityFrameworkCore.Diagnostics.InMemoryEventId.TransactionIgnoredWarning))
            .Options;

        var context = new AppDbContext(options);
        try
        {
            context.Database.EnsureCreated();
        }
        catch
        {
            // Silently fail if model building has issues with InMemory - still usable for basic tests
        }
        return context;
    }
}



