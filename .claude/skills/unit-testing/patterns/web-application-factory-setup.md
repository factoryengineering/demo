# WebApplicationFactory Setup (Integration Tests)

Patterns for writing integration tests that exercise the full ASP.NET Core pipeline in-process using `FestifyWebApplicationFactory`.

## How It Works

`FestifyWebApplicationFactory` extends `WebApplicationFactory<Program>`, which boots the application inside a `TestServer`. No TCP port is opened — `HttpClient.CreateClient()` returns a client whose transport is wired directly to the in-memory pipeline.

## The Factory

```csharp
public class FestifyWebApplicationFactory : WebApplicationFactory<Program>
{
    // Guid generated eagerly at construction time, before any DI resolution.
    // This guarantees every request within this factory's lifetime hits the
    // same database, and that two factories (one per test class) are isolated.
    private readonly string _dbName = Guid.NewGuid().ToString();

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        // ConfigureTestServices runs after Program.cs registrations,
        // so it overrides the production DbContext configuration.
        builder.ConfigureTestServices(services =>
        {
            services
                .Where(d => d.ServiceType == typeof(DbContextOptions<FestifyDbContext>))
                .ToList()
                .ForEach(d => services.Remove(d));

            services.AddDbContext<FestifyDbContext>(options =>
                options.UseInMemoryDatabase(_dbName));
        });
    }
}
```

## Test Class Structure

Use `IClassFixture<FestifyWebApplicationFactory>` so xUnit creates **one factory per test class**. Tests within a class run sequentially; the constructor resets state before each test.

```csharp
public class VenuesControllerTests : IClassFixture<FestifyWebApplicationFactory>
{
    private readonly HttpClient _client;
    private readonly FestifyWebApplicationFactory _factory;

    public VenuesControllerTests(FestifyWebApplicationFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();

        // Reset relevant tables before each test.
        // Open a scope against the factory's own service provider —
        // not a separate DbContext — so we clear the same database the server uses.
        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<FestifyDbContext>();
        db.Venues.RemoveRange(db.Venues);
        db.SaveChanges();
    }
}
```

## Key Rules

- **Guid in the field, not the lambda.** `Guid.NewGuid()` must be in the field initializer, not inside `ConfigureTestServices`. If placed in the lambda, it is evaluated lazily when the singleton `DbContextOptions` is first resolved — a point in time that may differ across parallel class initializations, producing different Guids for the same factory.

- **Use `factory.Services` for database resets**, not a separately constructed `FestifyDbContext`. The factory's `Services` property returns the root `IServiceProvider` of the test server, so clearing data through it affects the same store the server uses.

- **Remove `DbContextOptions<FestifyDbContext>` before re-registering.** `AddDbContext` uses `TryAdd` internally; if the production registration is not removed first, the override will be silently ignored.

- **One `HttpClient` per test class instance is sufficient.** `CreateClient()` always routes to the same `TestServer`; creating multiple clients is fine but not necessary for isolation.

## When to Add a New Test Class

Each new test class that uses `IClassFixture<FestifyWebApplicationFactory>` gets its own factory instance and therefore its own isolated database. No further configuration is needed.
