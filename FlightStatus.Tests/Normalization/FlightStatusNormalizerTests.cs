using FlightStatus.Models;
using FlightStatus.Normalization;
using FlightStatus.Providers.AeroTrack;
using FlightStatus.Providers.QuickFlight;

namespace FlightStatus.Tests.Normalization;

public class FlightStatusNormalizerTests
{
    private readonly FlightStatusNormalizer _normalizer = new();
    private static readonly DateTimeOffset ScheduledDeparture = Utc(2026, 7, 15, 10, 0);

    [Fact]
    public void FromAeroTrack_CancelledOverridesTiming()
    {
        var result = _normalizer.FromAeroTrack(new AeroTrackFlightStatusDto
        {
            Status = "Cancelled",
            ScheduledDepartureUtc = ScheduledDeparture,
            ActualDepartureUtc = ScheduledDeparture.AddHours(1)
        });

        Assert.Equal(Models.FlightStatus.Cancelled, result!.Status);
    }

    [Fact]
    public void FromAeroTrack_DepartureExactly15MinutesLate_IsOnTime()
    {
        var result = _normalizer.FromAeroTrack(new AeroTrackFlightStatusDto
        {
            Status = "Departed",
            ScheduledDepartureUtc = ScheduledDeparture,
            ActualDepartureUtc = ScheduledDeparture.AddMinutes(15)
        });

        Assert.Equal(Models.FlightStatus.OnTime, result!.Status);
    }

    [Fact]
    public void FromAeroTrack_Departure16MinutesLate_IsDelayed()
    {
        var result = _normalizer.FromAeroTrack(new AeroTrackFlightStatusDto
        {
            Status = "Departed",
            ScheduledDepartureUtc = ScheduledDeparture,
            ActualDepartureUtc = ScheduledDeparture.AddMinutes(16)
        });

        Assert.Equal(Models.FlightStatus.Delayed, result!.Status);
    }

    [Fact]
    public void FromAeroTrack_UsesEstimatedWhenActualMissing()
    {
        var result = _normalizer.FromAeroTrack(new AeroTrackFlightStatusDto
        {
            Status = "Estimated",
            ScheduledDepartureUtc = ScheduledDeparture,
            EstimatedDepartureUtc = ScheduledDeparture.AddMinutes(20)
        });

        Assert.Equal(Models.FlightStatus.Delayed, result!.Status);
    }

    [Fact]
    public void FromQuickFlight_DelayedVocabulary_ReturnsDelayed()
    {
        var result = _normalizer.FromQuickFlight(new QuickFlightFlightStatusDto
        {
            Status = "Delayed",
            ScheduledDepartureUtc = ScheduledDeparture
        });

        Assert.Equal(Models.FlightStatus.Delayed, result!.Status);
        Assert.Equal(QuickFlightStubProvider.ProviderId, result.SourceProvider);
    }

    [Fact]
    public void FromAeroTrack_Diverted_ReturnsDiverted()
    {
        var result = _normalizer.FromAeroTrack(new AeroTrackFlightStatusDto
        {
            Status = "Diverted",
            ScheduledDepartureUtc = ScheduledDeparture
        });

        Assert.Equal(Models.FlightStatus.Diverted, result!.Status);
    }

    [Fact]
    public void FromAeroTrack_ArrivalExactly15MinutesLate_IsOnTime()
    {
        var scheduledArrival = Utc(2026, 7, 15, 14, 0);

        var result = _normalizer.FromAeroTrack(new AeroTrackFlightStatusDto
        {
            Status = "Arrived",
            ScheduledArrivalUtc = scheduledArrival,
            ActualArrivalUtc = scheduledArrival.AddMinutes(15)
        });

        Assert.Equal(Models.FlightStatus.OnTime, result!.Status);
    }

    [Fact]
    public void FromAeroTrack_OnTimeVocabulary_ReturnsOnTime()
    {
        var result = _normalizer.FromAeroTrack(new AeroTrackFlightStatusDto
        {
            Status = "On Time",
            ScheduledDepartureUtc = ScheduledDeparture
        });

        Assert.Equal(Models.FlightStatus.OnTime, result!.Status);
    }

    [Fact]
    public void FromQuickFlight_Cancelled_ReturnsCancelled()
    {
        var result = _normalizer.FromQuickFlight(new QuickFlightFlightStatusDto
        {
            Status = "Cancelled",
            ScheduledDepartureUtc = ScheduledDeparture
        });

        Assert.Equal(Models.FlightStatus.Cancelled, result!.Status);
    }

    [Fact]
    public void FromQuickFlight_NoUsableStatus_ReturnsUnknown()
    {
        var result = _normalizer.FromQuickFlight(new QuickFlightFlightStatusDto
        {
            Status = "not-a-real-status",
            ScheduledDepartureUtc = ScheduledDeparture
        });

        Assert.Equal(Models.FlightStatus.Unknown, result!.Status);
    }

    private static DateTimeOffset Utc(int year, int month, int day, int hour, int minute) =>
        new(new DateOnly(year, month, day).ToDateTime(new TimeOnly(hour, minute)), TimeSpan.Zero);
}
