namespace Festify.Web.Models;

public record VenueResponse
{
    public Guid VenueGuid { get; init; }
    public string Name { get; init; } = string.Empty;
    public string Address { get; init; } = string.Empty;
    public double Latitude { get; init; }
    public double Longitude { get; init; }
    public int SeatingCapacity { get; init; }
    public string? Description { get; init; }
}
