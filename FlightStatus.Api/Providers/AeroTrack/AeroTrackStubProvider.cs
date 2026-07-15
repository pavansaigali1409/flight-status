using FlightStatus.Models;

namespace FlightStatus.Providers.AeroTrack;

public sealed class AeroTrackStubProvider : IFlightStatusProvider<AeroTrackFlightStatusDto>
{
    public const string ProviderId = "AeroTrack";

    private enum StubScenario
    {
        OnTime,
        Delayed,
        Cancelled,
        Diverted,
        NoData
    }

    public string ProviderName => ProviderId;

    public Task<ProviderFlightStatusResponse<AeroTrackFlightStatusDto>> GetFlightStatusAsync(
        FlightStatusQuery query,
        CancellationToken cancellationToken = default)
    {
        var scenario = ResolveScenario(query.FlightNumber, query.FlightDate);

        if (scenario == StubScenario.NoData)
        {
            return Task.FromResult(new ProviderFlightStatusResponse<AeroTrackFlightStatusDto>
            {
                ProviderName = ProviderName,
                Succeeded = false,
                Data = null,
                ErrorMessage = $"No flight data found for {query.FlightNumber} on {query.FlightDate:yyyy-MM-dd}."
            });
        }

        var data = BuildScenarioData(scenario, query.FlightDate);

        return Task.FromResult(new ProviderFlightStatusResponse<AeroTrackFlightStatusDto>
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

    private static AeroTrackFlightStatusDto BuildScenarioData(StubScenario scenario, DateOnly flightDate)
    {
        var scheduledDeparture = ToUtc(flightDate, hour: 10, minute: 0);
        var scheduledArrival = ToUtc(flightDate, hour: 14, minute: 0);
        var lastUpdated = ToUtc(flightDate, hour: 9, minute: 30);

        return scenario switch
        {
            StubScenario.OnTime => new AeroTrackFlightStatusDto
            {
                Status = "On Time",
                ScheduledDepartureUtc = scheduledDeparture,
                ScheduledArrivalUtc = scheduledArrival,
                ActualDepartureUtc = scheduledDeparture.AddMinutes(5),
                ActualArrivalUtc = scheduledArrival.AddMinutes(8),
                DepartureTerminal = "T1",
                ArrivalTerminal = "T2",
                DepartureGate = "A12",
                ArrivalGate = "B7",
                DelayReason = null,
                LastUpdatedUtc = lastUpdated
            },
            StubScenario.Delayed => new AeroTrackFlightStatusDto
            {
                Status = "Delayed",
                ScheduledDepartureUtc = scheduledDeparture,
                ScheduledArrivalUtc = scheduledArrival,
                ActualDepartureUtc = scheduledDeparture.AddMinutes(40),
                EstimatedArrivalUtc = scheduledArrival.AddMinutes(35),
                DepartureTerminal = "T1",
                ArrivalTerminal = "T2",
                DepartureGate = "C4",
                ArrivalGate = "D11",
                DelayReason = "Air traffic control",
                LastUpdatedUtc = lastUpdated.AddMinutes(45)
            },
            StubScenario.Cancelled => new AeroTrackFlightStatusDto
            {
                Status = "Cancelled",
                ScheduledDepartureUtc = scheduledDeparture,
                ScheduledArrivalUtc = scheduledArrival,
                DepartureTerminal = "T1",
                ArrivalTerminal = "T2",
                DepartureGate = "E2",
                ArrivalGate = null,
                DelayReason = "Operational reasons",
                LastUpdatedUtc = lastUpdated.AddHours(1)
            },
            StubScenario.Diverted => new AeroTrackFlightStatusDto
            {
                Status = "Diverted",
                ScheduledDepartureUtc = scheduledDeparture,
                ScheduledArrivalUtc = scheduledArrival,
                ActualDepartureUtc = scheduledDeparture.AddMinutes(2),
                ActualArrivalUtc = ToUtc(flightDate, hour: 15, minute: 20),
                DepartureTerminal = "T1",
                ArrivalTerminal = "T3",
                DepartureGate = "F9",
                ArrivalGate = "G1",
                DelayReason = "Weather at destination",
                LastUpdatedUtc = lastUpdated.AddHours(5)
            },
            _ => throw new ArgumentOutOfRangeException(nameof(scenario), scenario, "Unsupported stub scenario.")
        };
    }

    private static DateTimeOffset ToUtc(DateOnly date, int hour, int minute) =>
        new(date.ToDateTime(new TimeOnly(hour, minute)), TimeSpan.Zero);
}
