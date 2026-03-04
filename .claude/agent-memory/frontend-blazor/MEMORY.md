# Blazor Frontend Agent Memory — Festify.Web

See topic files for details. Key quick-reference items below.

## Project Location
- Web project: `Festify.Web/` (net10.0, Blazor Server SSR, no interactive render mode)
- API project: `Festify.Api/` (runs on http://localhost:5114)

## Architecture Summary
- Static SSR only — `AddRazorComponents()` with no `.AddInteractiveServerComponents()`. No `@rendermode` directives needed.
- Named HttpClient `"Festify.Api"` registered in `Program.cs`; base address from config `Festify:ApiBaseUrl`.
- Services registered in `Program.cs` as `AddScoped<TService>()`.
- Global namespaces in `Components/_Imports.razor` — add new `@using` statements here.

## Conventions
- Pages go in `Components/Pages/`, named `{Feature}.razor`, route `@page "/{feature}"`.
- Models go in `Models/`, services in `Services/`.
- `MainLayout.razor` is minimal — just `@Body`. No sidebar/navbar shell yet.
- CSS: single `wwwroot/app.css`; scoped CSS via `.razor.css` files for layout.
- No Bootstrap package referenced — only CSS variables like `var(--bs-secondary-color, #6c757d)` used as fallbacks.

## Patterns
See `patterns.md` for full details on: service result pattern, error/loading/empty states, retry links, navigation cards.

## Known API Facts
- `GET /api/venues` → 200 with `VenueResponse[]` (sorted by name asc). No auth currently enforced but UI handles 401.
- `VenueResponse`: venueGuid, name, address, latitude, longitude, seatingCapacity, description (nullable).
