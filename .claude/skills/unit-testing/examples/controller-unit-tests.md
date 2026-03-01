# Controller Unit Test Examples

Direct controller instantiation tests — no HTTP server, no `WebApplicationFactory`.

## When to Use This Approach

Use controller unit tests to verify business logic and data operations in isolation:
- Entity not found returns the correct result type
- Validation rules (e.g. capacity constraints)
- Data is correctly persisted and returned
- No dependency on routing, model binding, or middleware

## VenuesController Unit Tests

```csharp
public class VenuesControllerUnitTests : IDisposable
{
    private readonly FestifyDbContext _db;
    private readonly VenuesController _controller;

    public VenuesControllerUnitTests()
    {
        var options = new DbContextOptionsBuilder<FestifyDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        _db = new FestifyDbContext(options);
        _controller = new VenuesController(_db);
    }

    public void Dispose() => _db.Dispose();

    // Given helpers
    private Venue GivenVenue(
        string name = "Test Venue",
        string address = "123 Test St",
        int seatingCapacity = 1000)
    {
        return new Venue
        {
            VenueGuid = Guid.NewGuid(),
            Name = name,
            Address = address,
            Latitude = 41.85,
            Longitude = -87.65,
            SeatingCapacity = seatingCapacity
        };
    }

    [Fact]
    public async Task GetById_WhenVenueExists_ReturnsOkWithVenue()
    {
        // Given: A venue persisted in the database
        var venue = GivenVenue(name: "Metro Chicago");
        _db.Venues.Add(venue);
        await _db.SaveChangesAsync();

        // When: Getting the venue by its GUID
        var result = await _controller.GetById(venue.VenueGuid);

        // Then: OkObjectResult is returned containing the venue
        var ok = Assert.IsType<OkObjectResult>(result);
        var returned = Assert.IsType<Venue>(ok.Value);
        Assert.Equal("Metro Chicago", returned.Name);
    }

    [Fact]
    public async Task GetById_WhenVenueDoesNotExist_ReturnsNotFound()
    {
        // Given: No venues in the database

        // When: Getting a venue with an unknown GUID
        var result = await _controller.GetById(Guid.NewGuid());

        // Then: NotFoundResult is returned
        Assert.IsType<NotFoundResult>(result);
    }

    [Fact]
    public async Task Create_WithValidRequest_PersistsAndReturnsVenue()
    {
        // Given: A valid create request
        var request = new CreateVenueRequest
        {
            Name = "Metro Chicago",
            Address = "3730 N Clark St, Chicago, IL 60613",
            Latitude = 41.9497,
            Longitude = -87.6631,
            SeatingCapacity = 1100,
            Description = "Historic Chicago music venue since 1982."
        };

        // When: Creating the venue
        var result = await _controller.Create(request);

        // Then: CreatedAtActionResult is returned with the new venue
        var created = Assert.IsType<CreatedAtActionResult>(result);
        var venue = Assert.IsType<VenueResponse>(created.Value);
        Assert.NotEqual(Guid.Empty, venue.VenueGuid);
        Assert.Equal("Metro Chicago", venue.Name);

        // And: The venue is persisted
        var saved = await _db.Venues.FindAsync(venue.VenueGuid);
        Assert.NotNull(saved);
    }

    [Fact]
    public async Task Delete_WhenVenueExists_RemovesAndReturnsNoContent()
    {
        // Given: A venue in the database
        var venue = GivenVenue();
        _db.Venues.Add(venue);
        await _db.SaveChangesAsync();

        // When: Deleting the venue
        var result = await _controller.Delete(venue.VenueGuid);

        // Then: NoContentResult is returned
        Assert.IsType<NoContentResult>(result);

        // And: The venue is no longer in the database
        var deleted = await _db.Venues.FindAsync(venue.VenueGuid);
        Assert.Null(deleted);
    }
}
```

## What NOT to Test Here

These require the HTTP pipeline and belong in integration tests:

- HTTP status codes (204 vs 200 vs 404) as seen by an HTTP client
- `Location` header on 201 Created responses
- Request deserialization and model validation via `[ApiController]`
- Routing (e.g. `/api/venues/{guid}` resolves to `GetById`)
