namespace Festify.DataWarehouse.Models;

public class Event
{
    public int EventId { get; set; }
    public Guid EventGuid { get; set; }
    public string EventType { get; set; } = string.Empty;
    public DateTimeOffset OccurredAt { get; set; }
    public DateTimeOffset ReceivedAt { get; set; }
    public string Payload { get; set; } = string.Empty;
    public bool Processed { get; set; }
}
