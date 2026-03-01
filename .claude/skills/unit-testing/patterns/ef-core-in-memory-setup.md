# EF Core In-Memory Setup (Unit Tests)

Patterns for setting up `FestifyDbContext` with the InMemory provider for direct controller and service tests.

## Why InMemory for Unit Tests

- Tests real EF Core query logic without a database server
- No mocking of `DbSet` or `IQueryable` — use the real thing
- Fast: no I/O, no migrations
- Full relationship tracking and navigation property support

## Per-Test Isolation Pattern

Each test creates its own database with a unique name. Because `FestifyDbContext` is instantiated directly (not through DI), a new `Guid` is safe to use here — unlike in `WebApplicationFactory`, there is no singleton `DbContextOptions` to worry about.

```csharp
private FestifyDbContext CreateDbContext()
{
    var options = new DbContextOptionsBuilder<FestifyDbContext>()
        .UseInMemoryDatabase(Guid.NewGuid().ToString())
        .Options;
    return new FestifyDbContext(options);
}
```

## Async Disposal

Use `await using` so the context is disposed when the test ends:

```csharp
[Fact]
public async Task MyTest()
{
    await using var db = CreateDbContext();
    // ...
}
```

## What the InMemory Provider Does Not Support

- **Raw SQL**: `ExecuteSqlRaw`, `FromSqlRaw` — not supported
- **Database-generated values via SQL triggers or defaults** — not applied
- **Referential integrity enforcement** — FK constraints are not checked; the application layer must enforce them

For anything that requires a real database (complex joins, raw SQL, FK enforcement), write an integration test against a real provider instead.

## Constructor Setup (When Sharing Context Within a Test)

If multiple operations in one test need the same context, create it once and dispose via `IDisposable`:

```csharp
public class VenueControllerTests : IDisposable
{
    private readonly FestifyDbContext _db;
    private readonly VenuesController _controller;

    public VenueControllerTests()
    {
        var options = new DbContextOptionsBuilder<FestifyDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        _db = new FestifyDbContext(options);
        _controller = new VenuesController(_db);
    }

    public void Dispose() => _db.Dispose();
}
```

**Recommendation**: prefer `await using` per-test unless the test class is large and the setup is expensive.

## Package Requirements

```xml
<PackageReference Include="Microsoft.EntityFrameworkCore.InMemory" Version="10.*" />
```
