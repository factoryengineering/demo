using System.Net;
using System.Net.Http.Json;
using Festify.Api.Controllers;

namespace Festify.Tests;

public class VenuesControllerTests : IClassFixture<FestifyWebApplicationFactory>
{
    private readonly HttpClient client;

    public VenuesControllerTests(FestifyWebApplicationFactory factory)
    {
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
}
