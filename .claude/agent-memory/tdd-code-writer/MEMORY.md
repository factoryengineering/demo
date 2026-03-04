# TDD Code Writer Memory

## Project Structure
- Production API: `/Users/michaelperry/projects/ai/factoryengineering_demo/Festify.API/`
- Tests: `/Users/michaelperry/projects/ai/factoryengineering_demo/Festify.Tests/`
- Controllers: `/Users/michaelperry/projects/ai/factoryengineering_demo/Festify.API/Controllers/`

## Tech Stack
- .NET 10, ASP.NET Core Web API
- xUnit test framework
- Entity Framework Core with `FestifyDbContext`
- Integration tests use `FestifyWebApplicationFactory` (IClassFixture pattern)

## Test Execution
- Run all tests: `dotnet test /Users/michaelperry/projects/ai/factoryengineering_demo/Festify.Tests/Festify.Tests.csproj`
- Run single test: `dotnet test ... --filter "TestMethodName"`

## Controller Conventions
- `[ApiController]` + `[Route("api/[controller]")]` on the class
- Primary constructor injection (e.g., `VenuesController(FestifyDbContext db)`)
- Return `IActionResult` from actions
- DTOs (request/response records) defined in the same file as the controller
- `ToResponse()` private static helper converts entity to response record

## Coding Style
- C# records with `{ get; init; }` for DTOs
- `default!` for non-nullable string properties on records
- `async Task<IActionResult>` for DB-touching actions; plain `IActionResult` for in-memory returns
- `using Microsoft.AspNetCore.Mvc`, `Microsoft.EntityFrameworkCore`, `Festify.Api.Data`, `Festify.Api.Models`

## Patterns Observed
- Integration tests clear the DB in the constructor before each test class run
- `GivenVenue()` helper builds entity objects in tests (not production code)
- `CreatedAtAction(nameof(GetByGuid), ...)` used for POST responses
- LINQ query order: `.OrderBy(...)` placed before `.Select(...)` before `.ToListAsync()`

## Links
- See `patterns.md` for extended notes (if created)
