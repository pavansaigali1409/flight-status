using FlightStatus.Models;

namespace FlightStatus.Normalization;

public interface IFlightStatusMerger
{
    FlightStatusResult Merge(FlightStatusResult? aeroTrack, FlightStatusResult? quickFlight);
}
