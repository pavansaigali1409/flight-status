using FlightStatus.Models;
using FlightStatus.Providers.AeroTrack;
using FlightStatus.Providers.QuickFlight;

namespace FlightStatus.Normalization;

public sealed class FlightStatusNormalizer : IFlightStatusNormalizer
{
    private static readonly TimeSpan DelayThreshold = TimeSpan.FromMinutes(15);

    public FlightStatusResult? FromAeroTrack(AeroTrackFlightStatusDto? dto)
    {
        if (dto is null)
        {
            return null;
        }

        return new FlightStatusResult
        {
            Status = ResolveStatus(
                dto.Status,
                dto.ScheduledDepartureUtc,
                dto.ActualDepartureUtc ?? dto.EstimatedDepartureUtc,
                dto.ScheduledArrivalUtc,
                dto.ActualArrivalUtc ?? dto.EstimatedArrivalUtc),
            ScheduledDepartureUtc = dto.ScheduledDepartureUtc,
            ScheduledArrivalUtc = dto.ScheduledArrivalUtc,
            ActualDepartureUtc = dto.ActualDepartureUtc,
            ActualArrivalUtc = dto.ActualArrivalUtc,
            DepartureTerminal = dto.DepartureTerminal,
            ArrivalTerminal = dto.ArrivalTerminal,
            DepartureGate = dto.DepartureGate,
            ArrivalGate = dto.ArrivalGate,
            DelayReason = dto.DelayReason,
            LastUpdatedUtc = dto.LastUpdatedUtc,
            SourceProvider = AeroTrackStubProvider.ProviderId
        };
    }

    public FlightStatusResult? FromQuickFlight(QuickFlightFlightStatusDto? dto)
    {
        if (dto is null)
        {
            return null;
        }

        return new FlightStatusResult
        {
            Status = ResolveStatus(dto.Status, null, null, null, null),
            ScheduledDepartureUtc = dto.ScheduledDepartureUtc,
            ScheduledArrivalUtc = dto.ScheduledArrivalUtc,
            LastUpdatedUtc = dto.LastUpdatedUtc,
            SourceProvider = QuickFlightStubProvider.ProviderId
        };
    }

    private static Models.FlightStatus ResolveStatus(
        string? rawStatus,
        DateTimeOffset? scheduledDeparture,
        DateTimeOffset? effectiveDeparture,
        DateTimeOffset? scheduledArrival,
        DateTimeOffset? effectiveArrival)
    {
        if (TryParseExplicitOverride(rawStatus) is { } overrideStatus)
        {
            return overrideStatus;
        }

        if (TryEvaluateTiming(
                scheduledDeparture,
                effectiveDeparture,
                scheduledArrival,
                effectiveArrival) is { } timingStatus)
        {
            return timingStatus;
        }

        if (TryParseTimingStatus(rawStatus) is { } vocabularyStatus)
        {
            return vocabularyStatus;
        }

        return Models.FlightStatus.Unknown;
    }

    private static Models.FlightStatus? TryParseExplicitOverride(string? rawStatus)
    {
        if (string.IsNullOrWhiteSpace(rawStatus))
        {
            return null;
        }

        return rawStatus.Trim().ToLowerInvariant() switch
        {
            "cancelled" or "canceled" or "cancel" => Models.FlightStatus.Cancelled,
            "diverted" or "divert" => Models.FlightStatus.Diverted,
            _ => null
        };
    }

    private static Models.FlightStatus? TryParseTimingStatus(string? rawStatus)
    {
        if (string.IsNullOrWhiteSpace(rawStatus))
        {
            return null;
        }

        return rawStatus.Trim().ToLowerInvariant() switch
        {
            "delayed" or "delay" => Models.FlightStatus.Delayed,
            "on time" or "on-time" or "ontime" => Models.FlightStatus.OnTime,
            _ => null
        };
    }

    private static Models.FlightStatus? TryEvaluateTiming(
        DateTimeOffset? scheduledDeparture,
        DateTimeOffset? effectiveDeparture,
        DateTimeOffset? scheduledArrival,
        DateTimeOffset? effectiveArrival)
    {
        var hasDeparture = scheduledDeparture.HasValue && effectiveDeparture.HasValue;
        var hasArrival = scheduledArrival.HasValue && effectiveArrival.HasValue;

        if (!hasDeparture && !hasArrival)
        {
            return null;
        }

        if (IsLegDelayed(scheduledDeparture, effectiveDeparture)
            || IsLegDelayed(scheduledArrival, effectiveArrival))
        {
            return Models.FlightStatus.Delayed;
        }

        return Models.FlightStatus.OnTime;
    }

    private static bool IsLegDelayed(DateTimeOffset? scheduled, DateTimeOffset? effective) =>
        scheduled.HasValue
        && effective.HasValue
        && effective.Value > scheduled.Value.Add(DelayThreshold);
}
