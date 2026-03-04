# TDD Plan Strategist Memory

## Project: Festify

### Architecture
- ASP.NET Core Web API (.NET 10), project: `Festify.Api`
- Entity Framework Core with In-Memory database for tests
- Primary controller pattern: `ControllerBase` with `[ApiController]`, primary constructor injecting `FestifyDbContext`
- Route convention: `api/[controller]` for VenuesController, `[controller]` for EventsController (inconsistent)
- Models in `Festify.Api/Models/`, Data in `Festify.Api/Data/`, Controllers in `Festify.Api/Controllers/`
- Request/Response records defined in the controller file (e.g., `CreateVenueRequest`, `VenueResponse` in VenuesController.cs)
- Private static `ToResponse()` mapper method on controller

### Test Conventions
- xUnit with `IClassFixture<FestifyWebApplicationFactory>`
- `FestifyWebApplicationFactory` extends `WebApplicationFactory<Program>`, uses unique in-memory DB per test class
- Test class naming: `{Entity}ControllerTests`
- Test method naming: `{Action}_{ExpectedResult}` or `{Action}_{ExpectedResult}_{WhenCondition}` (PascalCase)
- Tests use `HttpClient` from factory, seed data via `factory.Services.CreateScope()` and direct DbContext access
- Arrange-Act-Assert pattern
- EventsControllerTests clears DB in constructor; VenuesControllerTests does NOT (potential shared state issue)
- Tests deserialize responses with `ReadFromJsonAsync<T>`

### Key Types
- `Venue` entity: VenueId (int PK), VenueGuid, Name, Address, Latitude, Longitude, SeatingCapacity, Description (nullable)
- `VenueResponse` record: VenueGuid, Name, Address, Latitude, Longitude, SeatingCapacity, Description (nullable)
- `FestifyDbContext`: DbSet<Event> Events, DbSet<Venue> Venues
- VenueId is internal only, never in VenueResponse

### Existing Endpoints
- `GET /api/venues/{venueGuid}` - GetByGuid
- `POST /api/venues` - Create
- `GET /api/venues` (list all) - NOT YET IMPLEMENTED
