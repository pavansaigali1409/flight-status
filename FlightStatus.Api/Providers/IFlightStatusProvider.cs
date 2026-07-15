using FlightStatus.Models;

namespace FlightStatus.Providers;

public interface IFlightStatusProvider<TRaw>
    where TRaw : class
{
    string ProviderName { get; }

    Task<ProviderFlightStatusResponse<TRaw>> GetFlightStatusAsync(
        FlightStatusQuery query,
        CancellationToken cancellationToken = default);
}
