using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Festify.Api.Data;
using Festify.Api.Models;

namespace Festify.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class VenuesController(FestifyDbContext db) : ControllerBase
{
    [HttpGet("{venueGuid}")]
    public async Task<IActionResult> GetByGuid(Guid venueGuid)
    {
        var venue = await db.Venues.FirstOrDefaultAsync(v => v.VenueGuid == venueGuid);
        if (venue is null) return NotFound();
        return Ok(ToResponse(venue));
    }

    [HttpPost]
    public async Task<IActionResult> Create(CreateVenueRequest request)
    {
        var venue = new Venue
        {
            VenueGuid = Guid.NewGuid(),
            Name = request.Name,
            Address = request.Address,
            Latitude = request.Latitude,
            Longitude = request.Longitude,
            SeatingCapacity = request.SeatingCapacity,
            Description = request.Description
        };

        db.Venues.Add(venue);
        await db.SaveChangesAsync();

        return CreatedAtAction(nameof(GetByGuid), new { venueGuid = venue.VenueGuid }, ToResponse(venue));
    }

    private static VenueResponse ToResponse(Venue venue) => new()
    {
        VenueGuid = venue.VenueGuid,
        Name = venue.Name,
        Address = venue.Address,
        Latitude = venue.Latitude,
        Longitude = venue.Longitude,
        SeatingCapacity = venue.SeatingCapacity,
        Description = venue.Description
    };
}

public record CreateVenueRequest
{
    public string Name { get; init; } = default!;
    public string Address { get; init; } = default!;
    public double Latitude { get; init; }
    public double Longitude { get; init; }
    public int SeatingCapacity { get; init; }
    public string? Description { get; init; }
}

public record VenueResponse
{
    public Guid VenueGuid { get; init; }
    public string Name { get; init; } = default!;
    public string Address { get; init; } = default!;
    public double Latitude { get; init; }
    public double Longitude { get; init; }
    public int SeatingCapacity { get; init; }
    public string? Description { get; init; }
}
