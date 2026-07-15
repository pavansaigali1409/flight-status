namespace FlightStatus.Models;

public sealed record FlightStatusQuery
{
    public required string FlightNumber { get; init; }
    public required DateOnly FlightDate { get; init; }
}
