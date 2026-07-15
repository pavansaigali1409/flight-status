# Flight Status API — Contracts

Data models and interface contracts

---

## Project layout

```
flight-status/
├── README.md                         
├── spec.md                            
├── FlightStatus.Api/
│   ├── FlightStatus.Api.csproj
│   ├── Program.cs
│   ├── Models/
│   │   ├── FlightStatus.cs
│   │   ├── StatusBasis.cs
│   │   ├── FlightStatusQuery.cs
│   │   └── FlightStatusResult.cs
│   ├── Normalization/
│   │   └── IFlightStatusNormalizer.cs
│   └── Providers/
│       ├── IFlightStatusProvider.cs
│       ├── ProviderFlightStatusResponse.cs
│       ├── AeroTrack/
│       │   ├── IAeroTrackProvider.cs
│       │   └── AeroTrackFlightStatusDto.cs
│       └── QuickFlight/
│           ├── IQuickFlightProvider.cs
│           └── QuickFlightFlightStatusDto.cs
├── FlightStatus.Tests/
│   ├── FlightStatus.Tests.csproj
│   └── Normalization/
│       └── (tests added during implementation)
├── <case-name>-ui/
├── prompts.md
└── reflection.md
```

Namespace convention: types under `FlightStatus.Api/` use root namespace `FlightStatus`.

---

## Normalization rules

| Status    | Rule |
|-----------|------|
| OnTime    | Departure or arrival within 15 minutes of schedule |
| Delayed   | Departure or arrival pushed beyond 15 minutes |
| Cancelled | Flight will not operate |
| Diverted  | Flight landed at a different airport |
| Unknown   | No usable status returned by either provider |

Priority: provider-explicit **Cancelled** / **Diverted** override timing rules.  
Merge: prefer AeroTrack (full detail); fall back to QuickFlight.  
AeroTrack effective times: `Actual ?? Estimated` for delay calculation.

---

## Models

### FlightStatus — unified enum

```csharp
// FlightStatus.Api/Models/FlightStatus.cs

namespace FlightStatus.Models;

/// <summary>
/// Normalized flight status after applying provider data and timing rules.
/// </summary>
public enum FlightStatus
{
    OnTime,
    Delayed,
    Cancelled,
    Diverted,
    Unknown
}
```

### StatusBasis — how status was determined

```csharp
// FlightStatus.Api/Models/StatusBasis.cs

namespace FlightStatus.Models;

/// <summary>
/// Explains how the normalized status was determined.
/// </summary>
public enum StatusBasis
{
    None,
    ProviderExplicit,   // Cancelled / Diverted from provider-native status
    DepartureTiming,    // Derived from departure schedule vs effective time
    ArrivalTiming       // Derived from arrival schedule vs effective time
}
```

### FlightStatusQuery — shared lookup contract

```csharp
// FlightStatus.Api/Models/FlightStatusQuery.cs

namespace FlightStatus.Models;

public sealed record FlightStatusQuery
{
    public required string AirlineCode { get; init; }
    public required string FlightNumber { get; init; }
    public required DateOnly FlightDate { get; init; }

    /// <summary>Optional disambiguation when a flight number repeats on the same day.</summary>
    public string? DepartureAirport { get; init; }
    public string? ArrivalAirport { get; init; }
}
```

### FlightStatusResult — unified output

Nullable detail fields reflect that QuickFlight cannot populate terminal, gate, actual times, or delay reason.

```csharp
// FlightStatus.Api/Models/FlightStatusResult.cs

namespace FlightStatus.Models;

public sealed record FlightStatusResult
{
    public required FlightStatus Status { get; init; }
    public StatusBasis StatusBasis { get; init; } = StatusBasis.None;

    public DateTimeOffset? ScheduledDepartureUtc { get; init; }
    public DateTimeOffset? ScheduledArrivalUtc { get; init; }

    /// <summary>Populated when AeroTrack (or merged data) supplies it.</summary>
    public DateTimeOffset? ActualDepartureUtc { get; init; }
    public DateTimeOffset? ActualArrivalUtc { get; init; }

    public string? DepartureTerminal { get; init; }
    public string? ArrivalTerminal { get; init; }
    public string? DepartureGate { get; init; }
    public string? ArrivalGate { get; init; }
    public string? DelayReason { get; init; }

    public DateTimeOffset? LastUpdatedUtc { get; init; }

    /// <summary>Provider that supplied the winning normalized data.</summary>
    public string? SourceProvider { get; init; }

    /// <summary>Original provider status before normalization.</summary>
    public string? RawProviderStatus { get; init; }
}
```

---

## Provider DTOs (raw responses)

### AeroTrack — full detail

Fields: status, scheduled/actual/estimated times, terminal, gate, delay reason, lastUpdatedUtc.

```csharp
// FlightStatus.Api/Providers/AeroTrack/AeroTrackFlightStatusDto.cs

namespace FlightStatus.Providers.AeroTrack;

/// <summary>
/// Raw response shape from AeroTrack. Status is provider-native text/code.
/// </summary>
public sealed record AeroTrackFlightStatusDto
{
    /// <summary>Provider-native status (e.g. "Delayed", "Cancelled", "Diverted").</summary>
    public string? Status { get; init; }

    public DateTimeOffset? ScheduledDepartureUtc { get; init; }
    public DateTimeOffset? ScheduledArrivalUtc { get; init; }

    public DateTimeOffset? ActualDepartureUtc { get; init; }
    public DateTimeOffset? ActualArrivalUtc { get; init; }

    /// <summary>Used when actual is not yet known (pre-departure / in-flight).</summary>
    public DateTimeOffset? EstimatedDepartureUtc { get; init; }
    public DateTimeOffset? EstimatedArrivalUtc { get; init; }

    public string? DepartureTerminal { get; init; }
    public string? ArrivalTerminal { get; init; }

    public string? DepartureGate { get; init; }
    public string? ArrivalGate { get; init; }

    public string? DelayReason { get; init; }

    public DateTimeOffset? LastUpdatedUtc { get; init; }
}
```

### QuickFlight — minimal

Fields: status, scheduled times, lastUpdatedUtc.

```csharp
// FlightStatus.Api/Providers/QuickFlight/QuickFlightFlightStatusDto.cs

namespace FlightStatus.Providers.QuickFlight;

/// <summary>
/// Raw response shape from QuickFlight. Status is provider-native text/code.
/// </summary>
public sealed record QuickFlightFlightStatusDto
{
    public string? Status { get; init; }

    public DateTimeOffset? ScheduledDepartureUtc { get; init; }
    public DateTimeOffset? ScheduledArrivalUtc { get; init; }

    public DateTimeOffset? LastUpdatedUtc { get; init; }
}
```

---

## Provider contracts (Option A)

Fetching only — no normalization logic in providers.

### ProviderFlightStatusResponse — wrapper

Distinguishes transport/no-data failures from `Unknown` normalized status.

```csharp
// FlightStatus.Api/Providers/ProviderFlightStatusResponse.cs

namespace FlightStatus.Providers;

public sealed record ProviderFlightStatusResponse<TRaw>
    where TRaw : class
{
    public required string ProviderName { get; init; }
    public required bool Succeeded { get; init; }
    public TRaw? Data { get; init; }
    public string? ErrorMessage { get; init; }
}
```

### IFlightStatusProvider — generic base

```csharp
// FlightStatus.Api/Providers/IFlightStatusProvider.cs

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
```

### Concrete provider interfaces

```csharp
// FlightStatus.Api/Providers/AeroTrack/IAeroTrackProvider.cs

namespace FlightStatus.Providers.AeroTrack;

public interface IAeroTrackProvider : IFlightStatusProvider<AeroTrackFlightStatusDto>
{
}
```

```csharp
// FlightStatus.Api/Providers/QuickFlight/IQuickFlightProvider.cs

namespace FlightStatus.Providers.QuickFlight;

public interface IQuickFlightProvider : IFlightStatusProvider<QuickFlightFlightStatusDto>
{
}
```

---

## Normalizer contract (separate from fetching)

```csharp
// FlightStatus.Api/Normalization/IFlightStatusNormalizer.cs

using FlightStatus.Models;
using FlightStatus.Providers.AeroTrack;
using FlightStatus.Providers.QuickFlight;

namespace FlightStatus.Normalization;

public interface IFlightStatusNormalizer
{
    /// <summary>
    /// Normalizes one or both provider responses into a unified result.
    /// Returns Unknown when neither provider supplies usable status.
    /// </summary>
    FlightStatusResult Normalize(
        AeroTrackFlightStatusDto? aeroTrack,
        QuickFlightFlightStatusDto? quickFlight);
}
```
