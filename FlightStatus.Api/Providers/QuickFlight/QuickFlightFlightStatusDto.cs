namespace FlightStatus.Providers.QuickFlight;

public sealed record QuickFlightFlightStatusDto
{
    public string? Status { get; init; }

    public DateTimeOffset? ScheduledDepartureUtc { get; init; }
    public DateTimeOffset? ScheduledArrivalUtc { get; init; }

    public DateTimeOffset? LastUpdatedUtc { get; init; }
}
