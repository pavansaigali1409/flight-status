using FlightStatus.Models;

namespace FlightStatus.Providers.QuickFlight;

public sealed class QuickFlightStubProvider : IFlightStatusProvider<QuickFlightFlightStatusDto>
{
    public const string ProviderId = "QuickFlight";

    private enum StubScenario
    {
        OnTime,
        Delayed,
        Cancelled,
        Diverted,
        NoData
    }

    public string ProviderName => ProviderId;

    public Task<ProviderFlightStatusResponse<QuickFlightFlightStatusDto>> GetFlightStatusAsync(
        FlightStatusQuery query,
        CancellationToken cancellationToken = default)
    {
        var scenario = ResolveScenario(query.FlightNumber, query.FlightDate);

        if (scenario == StubScenario.NoData)
        {
            return Task.FromResult(new ProviderFlightStatusResponse<QuickFlightFlightStatusDto>
            {
                ProviderName = ProviderName,
                Succeeded = false,
                Data = null,
                ErrorMessage = $"No flight data found for {query.FlightNumber} on {query.FlightDate:yyyy-MM-dd}."
            });
        }

        var data = BuildScenarioData(scenario, query.FlightDate);

        return Task.FromResult(new ProviderFlightStatusResponse<QuickFlightFlightStatusDto>
        {
            ProviderName = ProviderName,
            Succeeded = true,
            Data = data
        });
    }

    private static StubScenario ResolveScenario(string flightNumber, DateOnly flightDate)
    {
        var index = Math.Abs(HashCode.Combine(flightNumber, flightDate)) % 5;
        return (StubScenario)index;
    }

    private static QuickFlightFlightStatusDto BuildScenarioData(StubScenario scenario, DateOnly flightDate)
    {
        var scheduledDeparture = ToUtc(flightDate, hour: 10, minute: 0);
        var scheduledArrival = ToUtc(flightDate, hour: 14, minute: 0);
        var lastUpdated = ToUtc(flightDate, hour: 9, minute: 30);

        return scenario switch
        {
            StubScenario.OnTime => new QuickFlightFlightStatusDto
            {
                Status = "On Time",
                ScheduledDepartureUtc = scheduledDeparture,
                ScheduledArrivalUtc = scheduledArrival,
                LastUpdatedUtc = lastUpdated
            },
            StubScenario.Delayed => new QuickFlightFlightStatusDto
            {
                Status = "Delayed",
                ScheduledDepartureUtc = scheduledDeparture,
                ScheduledArrivalUtc = scheduledArrival,
                LastUpdatedUtc = lastUpdated.AddMinutes(45)
            },
            StubScenario.Cancelled => new QuickFlightFlightStatusDto
            {
                Status = "Cancelled",
                ScheduledDepartureUtc = scheduledDeparture,
                ScheduledArrivalUtc = scheduledArrival,
                LastUpdatedUtc = lastUpdated.AddHours(1)
            },
            StubScenario.Diverted => new QuickFlightFlightStatusDto
            {
                Status = "Diverted",
                ScheduledDepartureUtc = scheduledDeparture,
                ScheduledArrivalUtc = scheduledArrival,
                LastUpdatedUtc = lastUpdated.AddHours(5)
            },
            _ => throw new ArgumentOutOfRangeException(nameof(scenario), scenario, "Unsupported stub scenario.")
        };
    }

    private static DateTimeOffset ToUtc(DateOnly date, int hour, int minute) =>
        new(date.ToDateTime(new TimeOnly(hour, minute)), TimeSpan.Zero);
}
