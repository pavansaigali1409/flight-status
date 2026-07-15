using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc.Testing;

namespace FlightStatus.Tests.Integration;

public class FlightsStatusEndpointTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;

    public FlightsStatusEndpointTests(WebApplicationFactory<Program> factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task GetFlightsStatus_HappyPath_Returns200WithUnifiedResult()
    {
        var response = await _client.GetAsync("/flights/status?flightNumber=111&date=2026-07-15");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var json = await response.Content.ReadFromJsonAsync<JsonElement>();
        Assert.True(json.TryGetProperty("status", out var status));
        Assert.True(status.ValueKind == JsonValueKind.Number);
    }

    [Fact]
    public async Task GetFlightsStatus_MissingFlightNumber_Returns400()
    {
        var response = await _client.GetAsync("/flights/status?date=2026-07-15");

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

        var json = await response.Content.ReadFromJsonAsync<JsonElement>();
        Assert.Equal("flightNumber is required.", json.GetProperty("error").GetString());
    }

    [Fact]
    public async Task GetFlightsStatus_MissingDate_Returns400()
    {
        var response = await _client.GetAsync("/flights/status?flightNumber=111");

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

        var json = await response.Content.ReadFromJsonAsync<JsonElement>();
        Assert.Equal("date is required.", json.GetProperty("error").GetString());
    }

    [Fact]
    public async Task GetFlightsStatus_MalformedDate_Returns400()
    {
        var response = await _client.GetAsync("/flights/status?flightNumber=111&date=15-07-2026");

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

        var json = await response.Content.ReadFromJsonAsync<JsonElement>();
        Assert.Equal("date must be in yyyy-MM-dd format.", json.GetProperty("error").GetString());
    }
}
