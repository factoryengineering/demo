using Festify.Api.Models;

namespace Festify.Api.Data;

public static class SeedData
{
    public static void Initialize(FestifyDbContext db)
    {
        if (db.Venues.Any())
            return;

        var venues = new[]
        {
            new Venue
            {
                VenueId = 1,
                VenueGuid = Guid.Parse("11111111-1111-1111-1111-111111111101"),
                Name = "The Grand Ballroom",
                Address = "500 Festival Way, Austin, TX 78701",
                Latitude = 30.2672,
                Longitude = -97.7431,
                SeatingCapacity = 1200,
                Description = "Historic downtown venue with excellent acoustics."
            },
            new Venue
            {
                VenueId = 2,
                VenueGuid = Guid.Parse("22222222-2222-2222-2222-222222222202"),
                Name = "Riverside Amphitheatre",
                Address = "100 River Rd, Austin, TX 78704",
                Latitude = 30.2510,
                Longitude = -97.7550,
                SeatingCapacity = 3500,
                Description = "Outdoor amphitheatre on the Colorado River."
            },
            new Venue
            {
                VenueId = 3,
                VenueGuid = Guid.Parse("33333333-3333-3333-3333-333333333303"),
                Name = "Blue Note Lounge",
                Address = "200 E 6th St, Austin, TX 78701",
                Latitude = 30.2669,
                Longitude = -97.7400,
                SeatingCapacity = 200,
                Description = "Intimate jazz and blues club."
            },
            new Venue
            {
                VenueId = 4,
                VenueGuid = Guid.Parse("44444444-4444-4444-4444-444444444404"),
                Name = "Cactus Creek Pavilion",
                Address = "1500 S Congress Ave, Austin, TX 78704",
                Latitude = 30.2450,
                Longitude = -97.7520,
                SeatingCapacity = 800,
                Description = "Rustic indoor/outdoor space for festivals and private events."
            }
        };

        db.Venues.AddRange(venues);

        var events = new[]
        {
            new Event
            {
                Id = 1,
                Name = "Summer Jazz Night",
                Location = "Blue Note Lounge",
                StartDate = DateTime.UtcNow.AddDays(14),
                EndDate = DateTime.UtcNow.AddDays(14).AddHours(4),
                Capacity = 200
            },
            new Event
            {
                Id = 2,
                Name = "Riverside Rock Fest",
                Location = "Riverside Amphitheatre",
                StartDate = DateTime.UtcNow.AddDays(30),
                EndDate = DateTime.UtcNow.AddDays(31),
                Capacity = 3500
            },
            new Event
            {
                Id = 3,
                Name = "Festify Launch Gala",
                Location = "The Grand Ballroom",
                StartDate = DateTime.UtcNow.AddDays(7),
                EndDate = DateTime.UtcNow.AddDays(7).AddHours(5),
                Capacity = 1200
            },
            new Event
            {
                Id = 4,
                Name = "Cactus Country Fair",
                Location = "Cactus Creek Pavilion",
                StartDate = DateTime.UtcNow.AddDays(45),
                EndDate = DateTime.UtcNow.AddDays(47),
                Capacity = 800
            }
        };

        db.Events.AddRange(events);
        db.SaveChanges();
    }
}
