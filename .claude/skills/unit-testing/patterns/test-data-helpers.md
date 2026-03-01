# Test Data Helper Patterns

Patterns for `Given` helper methods that create Festify test entities.

## Core Rule: Navigation Properties, Not Foreign Keys

Always set navigation properties on child entities. Never set foreign key integers directly.

### ❌ Wrong — foreign keys

```csharp
private Show GivenShow(int venueId, int actId)
{
    return new Show { VenueId = venueId, ActId = actId, Date = DateTimeOffset.UtcNow.AddDays(7) };
}
```

EF Core won't track the relationship properly; navigation properties will be null; query behaviour may differ from production.

### ✅ Correct — navigation properties

```csharp
private Show GivenShow(Venue venue, Act act, DateTimeOffset? date = null)
{
    return new Show(venue, act)
    {
        ShowGuid = Guid.NewGuid(),
        Date = date ?? DateTimeOffset.UtcNow.AddDays(7)
    };
}
```

## Required vs Optional Parameters

| Parameter type | Rule | Reason |
|---|---|---|
| Parent navigation property | **Required** (no default) | Forces callers to be explicit about dependencies; compile-time safety |
| Scalar with sensible default | **Optional** (with default) | Keeps tests concise; only override what the test cares about |

## Festify Entity Helpers

### Venue (root entity — all optional)

```csharp
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
```

### Act (root entity — all optional)

```csharp
private Act GivenAct(
    Guid? actGuid = null,
    string name = "Test Act")
{
    return new Act
    {
        ActGuid = actGuid ?? Guid.NewGuid(),
        Name = name
    };
}
```

### Show (child of Venue and Act — parents required)

```csharp
private Show GivenShow(
    Venue venue,                        // Required parent
    Act act,                            // Required parent
    Guid? showGuid = null,
    DateTimeOffset? date = null)
{
    return new Show(venue, act)
    {
        ShowGuid = showGuid ?? Guid.NewGuid(),
        Date = date ?? DateTimeOffset.UtcNow.AddDays(7)
    };
}
```

### TicketSale (child of Show — parent required)

```csharp
private TicketSale GivenTicketSale(
    Show show,                          // Required parent
    Guid? ticketSaleGuid = null,
    int quantity = 2)
{
    return new TicketSale
    {
        TicketSaleGuid = ticketSaleGuid ?? Guid.NewGuid(),
        Show = show,                    // Navigation property, not ShowId
        Quantity = quantity
    };
}
```

## Anti-Patterns

### ❌ Separate builder classes

```csharp
// Avoid — unnecessary abstraction
var venue = new VenueBuilder().WithName("Metro").WithCapacity(1100).Build();
```

`Given` helpers with default parameters provide the same flexibility with less ceremony.

### ❌ Auto-creating parents inside helpers

```csharp
// Avoid — hides the dependency
private Show GivenShow(Venue? venue = null, Act? act = null)
{
    return new Show(venue ?? GivenVenue(), act ?? GivenAct()) { ... };
}
```

This makes it unclear what state is required and loses compile-time safety.

### ❌ Copying inline entity creation from other tests

If another test in the file uses `new Venue { ... }` (or similar) to create entities, do not copy that pattern. Use the Given helper for that entity type and add it to the test class if it is missing.
