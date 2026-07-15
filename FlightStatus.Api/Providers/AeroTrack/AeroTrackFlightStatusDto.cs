namespace FlightStatus.Providers.AeroTrack;

public sealed record AeroTrackFlightStatusDto
{
    public string? Status { get; init; }

    public DateTimeOffset? ScheduledDepartureUtc { get; init; }
    public DateTimeOffset? ScheduledArrivalUtc { get; init; }

    public DateTimeOffset? ActualDepartureUtc { get; init; }
    public DateTimeOffset? ActualArrivalUtc { get; init; }
    public DateTimeOffset? EstimatedDepartureUtc { get; init; }
    public DateTimeOffset? EstimatedArrivalUtc { get; init; }

    public string? DepartureTerminal { get; init; }
    public string? ArrivalTerminal { get; init; }
    public string? DepartureGate { get; init; }
    public string? ArrivalGate { get; init; }
    public string? DelayReason { get; init; }

    public DateTimeOffset? LastUpdatedUtc { get; init; }
}
