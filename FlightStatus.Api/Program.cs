using System.Globalization;
using FlightStatus.Models;
using FlightStatus.Normalization;
using FlightStatus.Providers;
using FlightStatus.Providers.AeroTrack;
using FlightStatus.Providers.QuickFlight;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSingleton<IFlightStatusProvider<AeroTrackFlightStatusDto>, AeroTrackStubProvider>();
builder.Services.AddSingleton<IFlightStatusProvider<QuickFlightFlightStatusDto>, QuickFlightStubProvider>();
builder.Services.AddSingleton<IFlightStatusNormalizer, FlightStatusNormalizer>();
builder.Services.AddSingleton<IFlightStatusMerger, FlightStatusMerger>();

builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
        policy.SetIsOriginAllowed(origin =>
                string.IsNullOrEmpty(origin)
                || origin.StartsWith("http://localhost", StringComparison.OrdinalIgnoreCase)
                || origin.StartsWith("http://127.0.0.1", StringComparison.OrdinalIgnoreCase))
            .AllowAnyHeader()
            .AllowAnyMethod());
});

var app = builder.Build();

app.UseCors();

app.MapGet("/flights/status", async (
    string? flightNumber,
    string? date,
    IFlightStatusProvider<AeroTrackFlightStatusDto> aeroTrackProvider,
    IFlightStatusProvider<QuickFlightFlightStatusDto> quickFlightProvider,
    IFlightStatusNormalizer normalizer,
    IFlightStatusMerger merger,
    CancellationToken cancellationToken) =>
{
    if (string.IsNullOrWhiteSpace(flightNumber))
    {
        return Results.BadRequest(new { error = "flightNumber is required." });
    }

    if (string.IsNullOrWhiteSpace(date))
    {
        return Results.BadRequest(new { error = "date is required." });
    }

    if (!DateOnly.TryParseExact(
            date,
            "yyyy-MM-dd",
            CultureInfo.InvariantCulture,
            DateTimeStyles.None,
            out var flightDate))
    {
        return Results.BadRequest(new { error = "date must be in yyyy-MM-dd format." });
    }

    var query = new FlightStatusQuery
    {
        FlightNumber = flightNumber.Trim(),
        FlightDate = flightDate
    };

    var aeroTrackResponse = await aeroTrackProvider.GetFlightStatusAsync(query, cancellationToken);
    var quickFlightResponse = await quickFlightProvider.GetFlightStatusAsync(query, cancellationToken);

    var aeroTrackResult = aeroTrackResponse.Succeeded
        ? normalizer.FromAeroTrack(aeroTrackResponse.Data)
        : null;

    var quickFlightResult = quickFlightResponse.Succeeded
        ? normalizer.FromQuickFlight(quickFlightResponse.Data)
        : null;

    return Results.Ok(merger.Merge(aeroTrackResult, quickFlightResult));
});

app.Run();

public partial class Program;
