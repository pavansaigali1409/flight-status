using FlightStatus.Models;

namespace FlightStatus.Normalization;

public sealed class FlightStatusMerger : IFlightStatusMerger
{
    private const string NoProviderDataMessage =
        "Neither AeroTrack nor QuickFlight returned usable flight data.";

    public FlightStatusResult Merge(FlightStatusResult? aeroTrack, FlightStatusResult? quickFlight)
    {
        if (aeroTrack is null && quickFlight is null)
        {
            return new FlightStatusResult
            {
                Status = Models.FlightStatus.Unknown,
                Message = NoProviderDataMessage
            };
        }

        if (aeroTrack is null)
        {
            return quickFlight!;
        }

        if (quickFlight is null)
        {
            return aeroTrack;
        }

        return SelectPreferred(aeroTrack, quickFlight);
    }

    private static FlightStatusResult SelectPreferred(
        FlightStatusResult aeroTrack,
        FlightStatusResult quickFlight)
    {
        var aeroTrackUpdated = aeroTrack.LastUpdatedUtc;
        var quickFlightUpdated = quickFlight.LastUpdatedUtc;

        if (aeroTrackUpdated is null && quickFlightUpdated is null)
        {
            return aeroTrack;
        }

        if (aeroTrackUpdated is null)
        {
            return quickFlight;
        }

        if (quickFlightUpdated is null)
        {
            return aeroTrack;
        }

        return aeroTrackUpdated >= quickFlightUpdated ? aeroTrack : quickFlight;
    }
}
