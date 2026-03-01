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
