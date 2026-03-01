namespace Festify.Api.Models;

public class Venue
{
    public int VenueId { get; set; }
    public Guid VenueGuid { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public double Latitude { get; set; }
    public double Longitude { get; set; }
    public int SeatingCapacity { get; set; }
    public string? Description { get; set; }
}
