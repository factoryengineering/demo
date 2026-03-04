# Festify

An event ticketing and show promotion management API built with ASP.NET Core and Entity Framework Core, with a Blazor UI (Festify.Web) that connects to the API.

## Prerequisites

- [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0)

## Build

```bash
dotnet build
```

## Run

### API only

```bash
dotnet run --project ./Festify.Api
```

The API starts on **http://localhost:5114** by default.

### Web UI only

```bash
dotnet run --project ./Festify.Web
```

The Web app starts on **http://localhost:5155** by default. It is configured to call the API at `http://localhost:5114` via `Festify:ApiBaseUrl` in `Festify.Web/appsettings.json` (and `appsettings.Development.json`). Change that value if the API runs on a different URL.

### Run API and Web together (so the Web app can connect to the API)

1. Start the API in one terminal:

   ```bash
   dotnet run --project ./Festify.Api
   ```

2. Start the Web app in a second terminal:

   ```bash
   dotnet run --project ./Festify.Web
   ```

3. Open **http://localhost:5155** in your browser. The Web app will use the API at **http://localhost:5114**. The API allows requests from the Web app origin via CORS (see `Festify.Api/appsettings.Development.json` → `Cors:AllowedOrigins`).

### API Reference

With the API running, open the Scalar API Reference page to browse and test endpoints interactively:

**http://localhost:5114/scalar/v1**

The raw OpenAPI document is available at:

**http://localhost:5114/openapi/v1.json**

## Test

```bash
dotnet test
```
