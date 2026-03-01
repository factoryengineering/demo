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

// Boots the full ASP.NET Core pipeline in-process using a TestServer — no real TCP port is
// opened. Requests made through clients it creates are routed through an in-memory transport,
// so tests run without starting an actual HTTP server.
public class FestifyWebApplicationFactory : WebApplicationFactory<Program>
{
    // Generated eagerly when the factory is constructed, before any DI container is built.
    // Storing it as a field guarantees that every call into ConfigureWebHost — and therefore
    // every DbContext resolved from this factory's container — uses the exact same name,
    // giving each factory instance its own isolated in-memory store. Because IClassFixture
    // creates one factory per test class, two test classes running in parallel each get a
    // distinct Guid and cannot see each other's data.
    private readonly string _dbName = Guid.NewGuid().ToString();

    // Called once when the test host is first built. ConfigureTestServices runs after the
    // application's own DI registrations, so anything registered here wins over Program.cs.
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureTestServices(services =>
        {
            // Remove the production DbContext options (which point to the "Festify"
            // in-memory database registered in Program.cs) so we can substitute our own.
            var toRemove = services
                .Where(d => d.ServiceType == typeof(DbContextOptions<FestifyDbContext>))
                .ToList();
            foreach (var d in toRemove) services.Remove(d);

            // Register the test database using the factory's fixed Guid name. DbContextOptions<T>
            // is a singleton, so all request scopes share the same store — data written in
            // one request is visible to subsequent requests within the same test.
            services.AddDbContext<FestifyDbContext>(options =>
                options.UseInMemoryDatabase(_dbName));
        });
    }
}

// IClassFixture<T> tells xUnit to create one FestifyWebApplicationFactory for the entire
// test class and inject it into every test constructor. This means the in-process server
// starts once and is reused, rather than being rebuilt per test.
public class EventsControllerTests : IClassFixture<FestifyWebApplicationFactory>
{
    private readonly HttpClient _client;
    private readonly FestifyWebApplicationFactory _factory;

    public EventsControllerTests(FestifyWebApplicationFactory factory)
    {
        _factory = factory;

        // CreateClient() returns an HttpClient whose transport is wired directly to the
        // TestServer pipeline — requests never leave the process.
        _client = factory.CreateClient();

        // Because the factory (and its in-memory database) is shared across all tests,
        // we reset the database state before each test to ensure isolation. A new DI
        // scope is opened here to avoid sharing a DbContext instance with the server.
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
