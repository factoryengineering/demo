# Integration Test Examples

Full HTTP pipeline tests using `FestifyWebApplicationFactory`. See also the existing `VenuesControllerTests` and `EventsControllerTests` in `Festify.Tests` for the canonical in-project examples.

## When to Use Integration Tests

Use integration tests to verify the HTTP contract:
- Correct status codes (201, 200, 404, 400, 409)
- Response body shape and field values
- `Location` header on 201 responses
- Model validation rejecting bad input (handled by `[ApiController]`)
- Route resolution

## Standard Test Class Structure

```csharp
public class VenuesControllerTests : IClassFixture<FestifyWebApplicationFactory>
{
    private readonly HttpClient _client;
    private readonly FestifyWebApplicationFactory _factory;

    public VenuesControllerTests(FestifyWebApplicationFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();

        // Clear relevant tables before each test for isolation.
        // Must use factory.Services — not a new DbContext — to target the
        // same in-memory store the test server uses.
        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<FestifyDbContext>();
        db.Venues.RemoveRange(db.Venues);
        db.SaveChanges();
    }

    // Given helpers produce request payloads, not entities —
    // we seed data through the HTTP API, not directly into the database.
    private CreateVenueRequest GivenCreateRequest(
        string name = "Test Venue",
        string address = "123 Test St",
        double latitude = 41.85,
        double longitude = -87.65,
        int seatingCapacity = 1000,
        string? description = null)
    {
        return new CreateVenueRequest
        {
            Name = name,
            Address = address,
            Latitude = latitude,
            Longitude = longitude,
            SeatingCapacity = seatingCapacity,
            Description = description
        };
    }

    [Fact]
    public async Task CreateVenue_WithValidRequest_Returns201WithLocationHeader()
    {
        // Given: A valid venue request
        var request = GivenCreateRequest(name: "Metro Chicago", seatingCapacity: 1100);

        // When: POSTing to /api/venues
        var response = await _client.PostAsJsonAsync("/api/venues", request);

        // Then: 201 Created is returned
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);

        // And: The response body contains the created venue with a server-assigned GUID
        var venue = await response.Content.ReadFromJsonAsync<VenueResponse>();
        Assert.NotNull(venue);
        Assert.NotEqual(Guid.Empty, venue!.VenueGuid);
        Assert.Equal("Metro Chicago", venue.Name);

        // And: The Location header points to the new resource
        Assert.NotNull(response.Headers.Location);
        Assert.Contains(venue.VenueGuid.ToString(), response.Headers.Location!.ToString());
    }

    [Fact]
    public async Task GetVenue_AfterCreate_Returns200WithSameData()
    {
        // Given: A venue has been created
        var request = GivenCreateRequest(name: "Jazz Club", seatingCapacity: 200);
        var created = await (await _client.PostAsJsonAsync("/api/venues", request))
            .Content.ReadFromJsonAsync<VenueResponse>();
        Assert.NotNull(created);

        // When: GETting the venue by its GUID
        var response = await _client.GetAsync($"/api/venues/{created!.VenueGuid}");

        // Then: 200 OK is returned with the same data
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var venue = await response.Content.ReadFromJsonAsync<VenueResponse>();
        Assert.Equal("Jazz Club", venue!.Name);
        Assert.Equal(200, venue.SeatingCapacity);
    }

    [Fact]
    public async Task GetVenue_WhenNotFound_Returns404()
    {
        // Given: No venues in the database

        // When: GETting a venue with an unknown GUID
        var response = await _client.GetAsync($"/api/venues/{Guid.NewGuid()}");

        // Then: 404 Not Found
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task CreateVenue_WithMissingRequiredField_Returns400()
    {
        // Given: A request with no name
        var request = new { Address = "123 St", Latitude = 41.85, Longitude = -87.65, SeatingCapacity = 100 };

        // When: POSTing the invalid request
        var response = await _client.PostAsJsonAsync("/api/venues", request);

        // Then: 400 Bad Request
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task DeleteVenue_WhenExists_Returns204AndIsGone()
    {
        // Given: A created venue
        var created = await (await _client.PostAsJsonAsync("/api/venues", GivenCreateRequest()))
            .Content.ReadFromJsonAsync<VenueResponse>();
        Assert.NotNull(created);

        // When: DELETEing the venue
        var deleteResponse = await _client.DeleteAsync($"/api/venues/{created!.VenueGuid}");

        // Then: 204 No Content
        Assert.Equal(HttpStatusCode.NoContent, deleteResponse.StatusCode);

        // And: A subsequent GET returns 404
        var getResponse = await _client.GetAsync($"/api/venues/{created.VenueGuid}");
        Assert.Equal(HttpStatusCode.NotFound, getResponse.StatusCode);
    }
}
```

## Seeding via the API vs Directly

**Prefer seeding through the API** (POST requests) in integration tests. This ensures your seed data travels through the same validation and mapping code as production requests.

**Use `factory.Services` to seed directly** only when you need to set up state that is not yet exposed via an endpoint (e.g. testing a read endpoint before the write endpoint exists).

```csharp
// Direct seed example — use sparingly
using var scope = _factory.Services.CreateScope();
var db = scope.ServiceProvider.GetRequiredService<FestifyDbContext>();
db.Venues.Add(new Venue { VenueGuid = Guid.NewGuid(), Name = "Seeded Venue", ... });
await db.SaveChangesAsync();
```
