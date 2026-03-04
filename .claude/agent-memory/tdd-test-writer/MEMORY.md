# TDD Test Writer Memory — Festify

## Project Structure

- **Solution root**: `/Users/michaelperry/projects/ai/factoryengineering_demo`
- **API project**: `Festify.Api/` — controllers, models, data
- **Test project**: `Festify.Tests/` — all tests here

## Test Runner

```
dotnet test Festify.Tests/Festify.Tests.csproj --filter "FullyQualifiedName~<TestName>" --logger "console;verbosity=detailed"
```

## Test Framework & Conventions

- **Framework**: xUnit with `WebApplicationFactory<Program>` for integration tests
- **DB**: EF Core InMemory via `FestifyWebApplicationFactory`
- **Pattern**: AAA with `// Given`, `// When`, `// Then` comments
- **Naming**: `MethodName_Scenario_ExpectedBehavior`
- **File naming**: `<Entity>ControllerTests.cs`
- **Assertion library**: `Assert.Equal`, `Assert.Empty`, `Assert.Single`, `Assert.NotNull`, etc. (xUnit)

## Key Files

- Skill docs: `.claude/skills/unit-testing/` — authoritative, always read before writing tests
- Factory class: defined in `Festify.Tests/EventsControllerTests.cs` as `FestifyWebApplicationFactory`
- Test files: `Festify.Tests/VenuesControllerTests.cs`, `Festify.Tests/EventsControllerTests.cs`
- Controllers: `Festify.Api/Controllers/VenuesController.cs`, `EventsController.cs`

## Integration Test Class Structure

- Implement `IClassFixture<FestifyWebApplicationFactory>`
- Fields: `private readonly HttpClient client;` and `private readonly FestifyWebApplicationFactory factory;` (no underscore prefix in `VenuesControllerTests`)
- Constructor must: create client, clear relevant DB tables via `factory.Services.CreateScope()`
- `GivenVenue(...)` helper in `VenuesControllerTests` returns a `Venue` entity (not a DTO)
- To seed: call `GivenVenue()`, then add/save via a `factory.Services.CreateScope()` block
- Prefer seeding via POST API; use direct DB seeding only when write endpoint is absent

## Skill Doc Rules (highest priority)

- Use `GivenVenue` / `GivenAct` / etc. helpers — never `new Entity { ... }` inline
- Navigation properties, not foreign key IDs in child entities
- Reset DB in constructor using `factory.Services`, not a separate DbContext
- Integration tests for HTTP contract; unit tests for business logic

## Observed Failure Modes

- Missing `[HttpGet]` collection action on controller → ASP.NET returns `405 MethodNotAllowed`
  - This is an `AssertionError` (`Expected: OK, Actual: MethodNotAllowed`) — correct failure for a missing `GetAll` endpoint
- Hardcoded empty list in GetAll endpoint → `Assert.Single() Failure: The collection was empty`
  - EF log `Saved 1 entities to in-memory store` confirms seed worked; controller ignores DB

## EF Core In-Memory and Case-Sensitive Sorting

- On macOS/.NET 10, EF Core In-Memory provider uses platform default string comparison, which is case-insensitive
- `OrderBy(v => v.Name)` with names "Alpha", "beta", "GAMMA" produces the correct case-insensitive order WITHOUT any explicit `StringComparer.OrdinalIgnoreCase`
- A case-insensitivity ordering test using these names will PASS even against a buggy implementation on macOS
- To write a test that genuinely catches a missing `StringComparer.OrdinalIgnoreCase`, names must be chosen where ASCII order and case-insensitive order differ AND the in-memory provider does not paper over the difference — this is very hard to guarantee without switching to SQLite
- If a case-sensitive ordering scenario is assigned and the in-memory provider masks the bug, report it honestly as a `PLAN_ASSUMPTION_WRONG` or in the test run result

## Mapping Verification Tests

- When a scenario asks to verify "all fields correctly mapped," the test may PASS if the implementation is already correct — the scenario spec explicitly allows for this outcome
- `VenuesController.ToResponse()` maps all seven Venue fields: VenueGuid, Name, Address, Latitude, Longitude, SeatingCapacity, Description — confirmed correct as of 2026-03-04
- Null `Description` is correctly serialized as JSON `null` by default ASP.NET Core serializer (no `JsonIgnoreCondition.WhenWritingNull` in `Program.cs`) — confirmed passing as of 2026-03-04
