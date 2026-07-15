namespace FlightStatus.Providers;

public sealed record ProviderFlightStatusResponse<TRaw>
    where TRaw : class
{
    public required string ProviderName { get; init; }
    public required bool Succeeded { get; init; }
    public TRaw? Data { get; init; }
    public string? ErrorMessage { get; init; }
}
