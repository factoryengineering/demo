using System.Net;
using System.Net.Http.Json;
using Festify.Api.Controllers;
using Festify.Api.Data;
using Festify.Api.Models;
using Microsoft.Extensions.DependencyInjection;

namespace Festify.Tests;

public class VenuesControllerTests : IClassFixture<FestifyWebApplicationFactory>
{
    private readonly HttpClient client;
    private readonly FestifyWebApplicationFactory factory;

    public VenuesControllerTests(FestifyWebApplicationFactory factory)
    {
        this.factory = factory;
        client = factory.CreateClient();

        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<FestifyDbContext>();
        db.Venues.RemoveRange(db.Venues);
        db.SaveChanges();
    }

    private Venue GivenVenue(
        Guid? venueGuid = null,
        string name = "Test Venue",
        string address = "123 Test St",
        double latitude = 41.85,
        double longitude = -87.65,
        int seatingCapacity = 1000,
        string? description = null)
    {
        return new Venue
        {
            VenueGuid = venueGuid ?? Guid.NewGuid(),
            Name = name,
            Address = address,
            Latitude = latitude,
            Longitude = longitude,
            SeatingCapacity = seatingCapacity,
            Description = description
        };
    }

    [Fact]
    public async Task CreateVenue_ReturnsCreatedWithVenueResponse()
    {
        var request = new CreateVenueRequest
        {
            Name = "Metro Chicago",
            Address = "3730 N Clark St, Chicago, IL 60613",
            Latitude = 41.9497,
            Longitude = -87.6631,
            SeatingCapacity = 1100,
            Description = "Historic Chicago music venue since 1982."
        };

        var response = await client.PostAsJsonAsync("/api/venues", request);

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);

        var venue = await response.Content.ReadFromJsonAsync<VenueResponse>();
        Assert.NotNull(venue);
        Assert.NotEqual(Guid.Empty, venue.VenueGuid);
        Assert.Equal(request.Name, venue.Name);
        Assert.Equal(request.Address, venue.Address);
        Assert.Equal(request.Latitude, venue.Latitude);
        Assert.Equal(request.Longitude, venue.Longitude);
        Assert.Equal(request.SeatingCapacity, venue.SeatingCapacity);
        Assert.Equal(request.Description, venue.Description);

        Assert.NotNull(response.Headers.Location);
        Assert.Contains(venue.VenueGuid.ToString(), response.Headers.Location!.ToString());

        var getResponse = await client.GetAsync($"/api/venues/{venue.VenueGuid}");
        Assert.Equal(HttpStatusCode.OK, getResponse.StatusCode);
    }

    [Fact]
    public async Task GetAll_ReturnsOkWithEmptyArray_WhenNoVenuesExist()
    {
        // Given: No venues in the database (cleared in constructor)

        // When: GET /api/venues is called
        var response = await client.GetAsync("/api/venues");

        // Then: 200 OK with an empty JSON array
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var venues = await response.Content.ReadFromJsonAsync<List<VenueResponse>>();
        Assert.Empty(venues!);
    }

    [Fact]
    public async Task GetAll_ReturnsSingleVenue_WhenOneVenueExists()
    {
        // Given: One venue seeded in the database
        var venue = GivenVenue(name: "Metro Chicago");
        using (var scope = factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<FestifyDbContext>();
            db.Venues.Add(venue);
            await db.SaveChangesAsync();
        }

        // When: GET /api/venues is called
        var response = await client.GetAsync("/api/venues");

        // Then: 200 OK with a JSON array containing exactly one venue
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var venues = await response.Content.ReadFromJsonAsync<List<VenueResponse>>();
        Assert.Single(venues!);
    }

    [Fact]
    public async Task GetAll_ReturnsAllVenueResponseFields_WhenVenueHasDescription()
    {
        // Given: A venue with all fields populated, including a non-null description
        var venueGuid = Guid.NewGuid();
        var venue = GivenVenue(
            venueGuid: venueGuid,
            name: "The Paramount",
            address: "2025 Broadway, Oakland, CA 94612",
            latitude: 37.8044,
            longitude: -122.2711,
            seatingCapacity: 3000,
            description: "Stunning Art Deco theater in Oakland.");
        using (var scope = factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<FestifyDbContext>();
            db.Venues.Add(venue);
            await db.SaveChangesAsync();
        }

        // When: GET /api/venues is called
        var response = await client.GetAsync("/api/venues");

        // Then: The single VenueResponse in the array contains all seven fields correctly mapped
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var venues = await response.Content.ReadFromJsonAsync<List<VenueResponse>>();
        var result = Assert.Single(venues!);
        Assert.Equal(venueGuid, result.VenueGuid);
        Assert.Equal("The Paramount", result.Name);
        Assert.Equal("2025 Broadway, Oakland, CA 94612", result.Address);
        Assert.Equal(37.8044, result.Latitude);
        Assert.Equal(-122.2711, result.Longitude);
        Assert.Equal(3000, result.SeatingCapacity);
        Assert.Equal("Stunning Art Deco theater in Oakland.", result.Description);
    }

    [Fact]
    public async Task GetAll_ReturnsNullDescription_WhenVenueHasNoDescription()
    {
        // Given: A venue with a null description seeded in the database
        var venue = GivenVenue(description: null);
        using (var scope = factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<FestifyDbContext>();
            db.Venues.Add(venue);
            await db.SaveChangesAsync();
        }

        // When: GET /api/venues is called
        var response = await client.GetAsync("/api/venues");

        // Then: The returned venue has a null description field (not empty string, not omitted)
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var venues = await response.Content.ReadFromJsonAsync<List<VenueResponse>>();
        var result = Assert.Single(venues!);
        Assert.Null(result.Description);
    }

    [Fact]
    public async Task GetAll_DoesNotReturnVenueId_InResponse()
    {
        // Given: One venue seeded in the database
        var venue = GivenVenue();
        using (var scope = factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<FestifyDbContext>();
            db.Venues.Add(venue);
            await db.SaveChangesAsync();
        }

        // When: GET /api/venues is called
        var response = await client.GetAsync("/api/venues");

        // Then: The raw JSON must not contain the internal integer primary key property name
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var json = await response.Content.ReadAsStringAsync();
        Assert.DoesNotContain("venueId", json, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task GetAll_ReturnsVenuesOrderedByNameAscending_WhenMultipleVenuesExist()
    {
        // Given: Three venues seeded in insertion order (not alphabetical)
        var venueGrandHall = GivenVenue(name: "The Grand Hall");
        var venueArenaEast = GivenVenue(name: "Arena East");
        var venueStudioB = GivenVenue(name: "Studio B");
        using (var scope = factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<FestifyDbContext>();
            db.Venues.Add(venueGrandHall);
            db.Venues.Add(venueArenaEast);
            db.Venues.Add(venueStudioB);
            await db.SaveChangesAsync();
        }

        // When: GET /api/venues is called
        var response = await client.GetAsync("/api/venues");

        // Then: The venues are returned in ascending alphabetical order by name
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var venues = await response.Content.ReadFromJsonAsync<List<VenueResponse>>();
        Assert.NotNull(venues);
        Assert.Equal(3, venues!.Count);
        Assert.Equal("Arena East", venues[0].Name);
        Assert.Equal("Studio B", venues[1].Name);
        Assert.Equal("The Grand Hall", venues[2].Name);
    }

    [Fact]
    public async Task GetAll_OrdersByNameCaseInsensitively()
    {
        // Given: Three venues seeded with names that differ only in casing, inserted out of case-insensitive order
        var venueBeta = GivenVenue(name: "beta");
        var venueAlpha = GivenVenue(name: "Alpha");
        var venueGamma = GivenVenue(name: "GAMMA");
        using (var scope = factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<FestifyDbContext>();
            db.Venues.Add(venueBeta);
            db.Venues.Add(venueAlpha);
            db.Venues.Add(venueGamma);
            await db.SaveChangesAsync();
        }

        // When: GET /api/venues is called
        var response = await client.GetAsync("/api/venues");

        // Then: The venues are returned in case-insensitive ascending order: Alpha, beta, GAMMA
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var venues = await response.Content.ReadFromJsonAsync<List<VenueResponse>>();
        Assert.NotNull(venues);
        Assert.Equal(3, venues!.Count);
        Assert.Equal("Alpha", venues[0].Name);
        Assert.Equal("beta", venues[1].Name);
        Assert.Equal("GAMMA", venues[2].Name);
    }

    [Fact]
    public async Task GetAll_ReturnsStableOrdering_OnRepeatedCalls()
    {
        // Given: Three venues with distinct names seeded in the database
        var venueZephyr = GivenVenue(name: "Zephyr Hall");
        var venueAnchor = GivenVenue(name: "Anchor Arena");
        var venueMidway = GivenVenue(name: "Midway Stage");
        using (var scope = factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<FestifyDbContext>();
            db.Venues.Add(venueZephyr);
            db.Venues.Add(venueAnchor);
            db.Venues.Add(venueMidway);
            await db.SaveChangesAsync();
        }

        // When: GET /api/venues is called twice
        var response1 = await client.GetAsync("/api/venues");
        var response2 = await client.GetAsync("/api/venues");

        // Then: Both responses return the same venue names in the same order
        var venues1 = await response1.Content.ReadFromJsonAsync<List<VenueResponse>>();
        var venues2 = await response2.Content.ReadFromJsonAsync<List<VenueResponse>>();
        Assert.NotNull(venues1);
        Assert.NotNull(venues2);
        Assert.Equal(venues1!.Count, venues2!.Count);
        for (int i = 0; i < venues1.Count; i++)
        {
            Assert.Equal(venues1[i].Name, venues2[i].Name);
        }
    }

    [Fact]
    public async Task GetAll_ReturnsVenuesWithSameName_InStableOrder()
    {
        // Given: Two venues with identical names but different VenueGuids seeded in the database
        var guid1 = Guid.NewGuid();
        var guid2 = Guid.NewGuid();
        var venue1 = GivenVenue(venueGuid: guid1, name: "Stage One");
        var venue2 = GivenVenue(venueGuid: guid2, name: "Stage One");
        using (var scope = factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<FestifyDbContext>();
            db.Venues.Add(venue1);
            db.Venues.Add(venue2);
            await db.SaveChangesAsync();
        }

        // When: GET /api/venues is called twice
        var response1 = await client.GetAsync("/api/venues");
        var response2 = await client.GetAsync("/api/venues");

        // Then: Both calls return 2 items and the same VenueGuid appears at index 0 in both
        var venues1 = await response1.Content.ReadFromJsonAsync<List<VenueResponse>>();
        var venues2 = await response2.Content.ReadFromJsonAsync<List<VenueResponse>>();
        Assert.NotNull(venues1);
        Assert.NotNull(venues2);
        Assert.Equal(2, venues1!.Count);
        Assert.Equal(2, venues2!.Count);
        Assert.Equal(venues1[0].VenueGuid, venues2[0].VenueGuid);
    }

    [Fact]
    public async Task GetAll_ReturnsOkWithEmptyArray_AfterAllVenuesAreDeleted()
    {
        // Given: Two venues seeded in the database, then both removed via FestifyDbContext
        var venue1 = GivenVenue(name: "Vanishing Act A");
        var venue2 = GivenVenue(name: "Vanishing Act B");
        using (var scope = factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<FestifyDbContext>();
            db.Venues.Add(venue1);
            db.Venues.Add(venue2);
            await db.SaveChangesAsync();
        }
        using (var scope = factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<FestifyDbContext>();
            db.Venues.RemoveRange(db.Venues);
            await db.SaveChangesAsync();
        }

        // When: GET /api/venues is called
        var response = await client.GetAsync("/api/venues");

        // Then: 200 OK with an empty JSON array
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var venues = await response.Content.ReadFromJsonAsync<List<VenueResponse>>();
        Assert.Empty(venues!);
    }

    [Fact]
    public async Task GetByGuid_ReturnsVenueWithAllFields()
    {
        var venueGuid = Guid.NewGuid();
        using (var scope = factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<FestifyDbContext>();
            db.Venues.Add(new Venue
            {
                VenueGuid = venueGuid,
                Name = "The Fillmore",
                Address = "1805 Geary Blvd, San Francisco, CA 94115",
                Latitude = 37.7842,
                Longitude = -122.4332,
                SeatingCapacity = 1150,
                Description = "Legendary SF music venue."
            });
            await db.SaveChangesAsync();
        }

        var response = await client.GetAsync($"/api/venues/{venueGuid}");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var venue = await response.Content.ReadFromJsonAsync<VenueResponse>();
        Assert.NotNull(venue);
        Assert.Equal(venueGuid, venue.VenueGuid);
        Assert.Equal("The Fillmore", venue.Name);
        Assert.Equal("1805 Geary Blvd, San Francisco, CA 94115", venue.Address);
        Assert.Equal(37.7842, venue.Latitude);
        Assert.Equal(-122.4332, venue.Longitude);
        Assert.Equal(1150, venue.SeatingCapacity);
        Assert.Equal("Legendary SF music venue.", venue.Description);
    }
}
