# Blazor Frontend Patterns — Festify.Web

## HttpClient Usage
Named client `"Festify.Api"` via `IHttpClientFactory.CreateClient("Festify.Api")`.
Base address already set; use relative paths like `/api/venues`.
Never inject `HttpClient` directly — always via factory.

## Service Result Pattern
Use a result record/class with a `Status` enum and typed payload. Example:

```csharp
public enum VenueLoadStatus { Success, Unauthorized, ServerError, NetworkError }

public class VenueLoadResult {
    public VenueLoadStatus Status { get; init; }
    public IReadOnlyList<VenueResponse> Venues { get; init; } = [];
    public string? ErrorMessage { get; init; }
    // static factory methods: Ok(), Failure()
}
```

Catch `HttpRequestException` for network errors. Check `HttpStatusCode.Unauthorized` explicitly.
All other non-success codes → ServerError. JSON deserialization exceptions → ServerError.

## Loading / Error / Empty States in Pages
- `_result is null` → loading text
- `_result.Status == Unauthorized` → error div with sign-in link
- `_result.Status == NetworkError` → error div with connection message + Retry link
- `_result.Status == ServerError` → error div with "could not be loaded" + Retry link
- `_result.Status == Success && !venues.Any()` → empty-state message inside the scrollable container
- `_result.Status == Success && venues.Any()` → render list

Retry link is `<a href="/current-page">Retry</a>` (SSR page reload).

## Error State CSS
`.venues-error` — red-tinted alert box (border #f5c2c7, bg #f8d7da, color #842029).
`.btn-link` — inline link styled to match error color.

## Navigation Cards (Home Page)
`.nav-cards` — flex row, wraps, centered.
`.nav-card` — bordered card (1px #dee2e6, radius 0.5rem), links to a page.
`.nav-card-title` — bold, larger text.
`.nav-card-description` — secondary/muted text.

## Scrollable List Container
`.venues-list` — `max-height: 70vh; overflow-y: auto;` with a border.
`.venue-item` — padded row with bottom border (removed from last child).
`.venue-name` — `white-space: normal; word-break: break-word` so long/special names wrap rather than clip.

## Service Registration
`builder.Services.AddScoped<TService>()` in `Program.cs`.
Add `@using Festify.Web.Services` and `@using Festify.Web.Models` to `Components/_Imports.razor`.
