using FlightStatus.Models;
using FlightStatus.Providers.AeroTrack;
using FlightStatus.Providers.QuickFlight;

namespace FlightStatus.Normalization;

public interface IFlightStatusNormalizer
{
    FlightStatusResult? FromAeroTrack(AeroTrackFlightStatusDto? dto);

    FlightStatusResult? FromQuickFlight(QuickFlightFlightStatusDto? dto);
}
