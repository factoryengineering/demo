using System.Net;
using System.Net.Http.Json;
using Festify.Web.Models;

namespace Festify.Web.Services;

public enum VenueLoadStatus
{
    Success,
    Unauthorized,
    ServerError,
    NetworkError
}

public class VenueLoadResult
{
    public VenueLoadStatus Status { get; init; }
    public IReadOnlyList<VenueResponse> Venues { get; init; } = [];
    public string? ErrorMessage { get; init; }

    public static VenueLoadResult Ok(IReadOnlyList<VenueResponse> venues) =>
        new() { Status = VenueLoadStatus.Success, Venues = venues };

    public static VenueLoadResult Failure(VenueLoadStatus status, string message) =>
        new() { Status = status, ErrorMessage = message };
}

public class VenueService(IHttpClientFactory httpClientFactory)
{
    private readonly HttpClient _http = httpClientFactory.CreateClient("Festify.Api");

    public async Task<VenueLoadResult> GetVenuesAsync()
    {
        try
        {
            var response = await _http.GetAsync("/api/venues");

            if (response.StatusCode == HttpStatusCode.Unauthorized)
                return VenueLoadResult.Failure(VenueLoadStatus.Unauthorized,
                    "You must be signed in to view venues.");

            if (!response.IsSuccessStatusCode)
                return VenueLoadResult.Failure(VenueLoadStatus.ServerError,
                    "Venues could not be loaded.");

            var venues = await response.Content.ReadFromJsonAsync<List<VenueResponse>>();
            return VenueLoadResult.Ok(venues ?? []);
        }
        catch (HttpRequestException)
        {
            return VenueLoadResult.Failure(VenueLoadStatus.NetworkError,
                "Could not connect to the server. Please check your connection.");
        }
        catch (Exception)
        {
            return VenueLoadResult.Failure(VenueLoadStatus.ServerError,
                "Venues could not be loaded.");
        }
    }
}
