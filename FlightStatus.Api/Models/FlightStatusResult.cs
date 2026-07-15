namespace FlightStatus.Models;

public sealed record FlightStatusResult
{
    public required FlightStatus Status { get; init; }

    public DateTimeOffset? ScheduledDepartureUtc { get; init; }
    public DateTimeOffset? ScheduledArrivalUtc { get; init; }
    public DateTimeOffset? ActualDepartureUtc { get; init; }
    public DateTimeOffset? ActualArrivalUtc { get; init; }

    public string? DepartureTerminal { get; init; }
    public string? ArrivalTerminal { get; init; }
    public string? DepartureGate { get; init; }
    public string? ArrivalGate { get; init; }
    public string? DelayReason { get; init; }

    public DateTimeOffset? LastUpdatedUtc { get; init; }
    public string? SourceProvider { get; init; }
    public string? Message { get; init; }
}
