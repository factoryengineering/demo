using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Festify.Api.Data;
using Festify.Api.Models;

namespace Festify.Tests;

public class FestifyWebApplicationFactory : WebApplicationFactory<Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureTestServices(services =>
        {
            var toRemove = services
                .Where(d => d.ServiceType == typeof(DbContextOptions<FestifyDbContext>))
                .ToList();
            foreach (var d in toRemove) services.Remove(d);

            services.AddDbContext<FestifyDbContext>(options =>
                options.UseInMemoryDatabase("FestifyTest"));
        });
    }
}

public class EventsControllerTests : IClassFixture<FestifyWebApplicationFactory>
{
    private readonly HttpClient _client;
    private readonly FestifyWebApplicationFactory _factory;

    public EventsControllerTests(FestifyWebApplicationFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();

        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<FestifyDbContext>();
        db.Events.RemoveRange(db.Events);
        db.SaveChanges();
    }

    [Fact]
    public async Task GetAll_ReturnsEmptyList_WhenNoEvents()
    {
        var response = await _client.GetAsync("/events");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var events = await response.Content.ReadFromJsonAsync<List<Event>>();
        Assert.Empty(events!);
    }

    [Fact]
    public async Task Create_ReturnsCreatedEvent()
    {
        var newEvent = new Event
        {
            Name = "Summer Fest",
            Location = "Central Park",
            StartDate = new DateTime(2026, 7, 1),
            EndDate = new DateTime(2026, 7, 3),
            Capacity = 5000
        };

        var response = await _client.PostAsJsonAsync("/events", newEvent);

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var created = await response.Content.ReadFromJsonAsync<Event>();
        Assert.Equal("Summer Fest", created!.Name);
        Assert.True(created.Id > 0);
    }

    [Fact]
    public async Task GetById_ReturnsEvent_WhenExists()
    {
        var newEvent = new Event
        {
            Name = "Jazz Night",
            Location = "Blue Note",
            StartDate = new DateTime(2026, 8, 15),
            EndDate = new DateTime(2026, 8, 15),
            Capacity = 200
        };

        var postResponse = await _client.PostAsJsonAsync("/events", newEvent);
        Assert.Equal(HttpStatusCode.Created, postResponse.StatusCode);
        var created = await postResponse.Content.ReadFromJsonAsync<Event>();
        Assert.NotNull(created);
        Assert.True(created.Id > 0);

        var response = await _client.GetAsync($"/events/{created.Id}");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var ev = await response.Content.ReadFromJsonAsync<Event>();
        Assert.Equal("Jazz Night", ev!.Name);
    }

    [Fact]
    public async Task GetById_ReturnsNotFound_WhenMissing()
    {
        var response = await _client.GetAsync("/events/99999");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task Delete_RemovesEvent()
    {
        var newEvent = new Event
        {
            Name = "Rock Fest",
            Location = "Stadium",
            StartDate = new DateTime(2026, 9, 1),
            EndDate = new DateTime(2026, 9, 2),
            Capacity = 10000
        };

        var postResponse = await _client.PostAsJsonAsync("/events", newEvent);
        Assert.Equal(HttpStatusCode.Created, postResponse.StatusCode);
        var created = await postResponse.Content.ReadFromJsonAsync<Event>();
        Assert.NotNull(created);
        Assert.True(created.Id > 0);

        var deleteResponse = await _client.DeleteAsync($"/events/{created.Id}");
        Assert.Equal(HttpStatusCode.NoContent, deleteResponse.StatusCode);

        var getResponse = await _client.GetAsync($"/events/{created.Id}");
        Assert.Equal(HttpStatusCode.NotFound, getResponse.StatusCode);
    }

    [Fact]
    public async Task Update_ModifiesEvent()
    {
        var newEvent = new Event
        {
            Name = "Original Name",
            Location = "Venue",
            StartDate = new DateTime(2026, 10, 1),
            EndDate = new DateTime(2026, 10, 1),
            Capacity = 100
        };

        var postResponse = await _client.PostAsJsonAsync("/events", newEvent);
        Assert.Equal(HttpStatusCode.Created, postResponse.StatusCode);
        var created = await postResponse.Content.ReadFromJsonAsync<Event>();
        Assert.NotNull(created);
        Assert.True(created.Id > 0);

        created.Name = "Updated Name";
        var updateResponse = await _client.PutAsJsonAsync($"/events/{created.Id}", created);
        Assert.Equal(HttpStatusCode.NoContent, updateResponse.StatusCode);

        var ev = await (await _client.GetAsync($"/events/{created.Id}"))
            .Content.ReadFromJsonAsync<Event>();
        Assert.Equal("Updated Name", ev!.Name);
    }
}
