using FlightStatus.Models;
using FlightStatus.Normalization;
using FlightStatus.Providers.AeroTrack;
using FlightStatus.Providers.QuickFlight;

namespace FlightStatus.Tests.Normalization;

public class FlightStatusMergerTests
{
    private readonly FlightStatusMerger _merger = new();

    [Fact]
    public void Merge_BothRespond_PrefersLaterLastUpdatedUtc()
    {
        var aeroTrack = CreateResult(Models.FlightStatus.OnTime, AeroTrackStubProvider.ProviderId, Utc(9, 30));
        var quickFlight = CreateResult(Models.FlightStatus.Delayed, QuickFlightStubProvider.ProviderId, Utc(10, 15));

        var result = _merger.Merge(aeroTrack, quickFlight);

        Assert.Equal(QuickFlightStubProvider.ProviderId, result.SourceProvider);
    }

    [Fact]
    public void Merge_TieOnLastUpdatedUtc_PrefersAeroTrack()
    {
        var lastUpdated = Utc(12, 0);
        var aeroTrack = CreateResult(Models.FlightStatus.OnTime, AeroTrackStubProvider.ProviderId, lastUpdated);
        var quickFlight = CreateResult(Models.FlightStatus.Delayed, QuickFlightStubProvider.ProviderId, lastUpdated);

        var result = _merger.Merge(aeroTrack, quickFlight);

        Assert.Equal(AeroTrackStubProvider.ProviderId, result.SourceProvider);
    }

    [Fact]
    public void Merge_OnlyAeroTrackResponds_ReturnsAeroTrack()
    {
        var aeroTrack = CreateResult(Models.FlightStatus.Cancelled, AeroTrackStubProvider.ProviderId, Utc(10, 0));

        Assert.Same(aeroTrack, _merger.Merge(aeroTrack, null));
    }

    [Fact]
    public void Merge_OnlyQuickFlightResponds_ReturnsQuickFlight()
    {
        var quickFlight = CreateResult(Models.FlightStatus.Delayed, QuickFlightStubProvider.ProviderId, Utc(10, 0));

        Assert.Same(quickFlight, _merger.Merge(null, quickFlight));
    }

    [Fact]
    public void Merge_NeitherResponds_ReturnsUnknownWithMessage()
    {
        var result = _merger.Merge(null, null);

        Assert.Equal(Models.FlightStatus.Unknown, result.Status);
        Assert.Equal("Neither AeroTrack nor QuickFlight returned usable flight data.", result.Message);
    }

    private static FlightStatusResult CreateResult(
        Models.FlightStatus status,
        string sourceProvider,
        DateTimeOffset? lastUpdatedUtc) =>
        new()
        {
            Status = status,
            SourceProvider = sourceProvider,
            LastUpdatedUtc = lastUpdatedUtc
        };

    private static DateTimeOffset Utc(int hour, int minute) =>
        new(new DateOnly(2026, 7, 15).ToDateTime(new TimeOnly(hour, minute)), TimeSpan.Zero);
}
