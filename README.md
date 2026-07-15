# flight-status

Unified flight status API merging AeroTrack (full detail) and QuickFlight (minimal) into one response.

## Run

**API**
```bash
dotnet run --project FlightStatus.Api
```
Listens on `http://localhost:5000`

**UI** — serve `flight-status-ui/` over HTTP (e.g. Live Server, `npx serve flight-status-ui`), then open in browser.

**Tests**
```bash
dotnet test FlightStatus.Tests
```

## Try it

```
GET /flights/status?flightNumber=111&date=2026-07-15
```

Stub responses are deterministic: same flight number + date always returns the same scenario.

## Assumptions

- `flightNumber` is passed as a single query parameter (e.g. `BA100` or `111`); no separate airline-code field.
- Stub providers are deterministic: scenario is chosen from `HashCode.Combine(flightNumber, date) % 5`, covering OnTime, Delayed, Cancelled, Diverted, and NoData.
- QuickFlight has no actual/estimated times, so its status is derived from provider vocabulary only.
- When both providers succeed with the same `lastUpdatedUtc`, AeroTrack is preferred (tie-break for richer detail).
- The UI must be served over HTTP (not `file://`) so browser fetch/CORS works; API listens on port 5000.

## Structure

- `FlightStatus.Api/Models` — unified contracts
- `FlightStatus.Api/Providers` — raw DTOs + stub providers
- `FlightStatus.Api/Normalization` — status rules + merge logic
- `FlightStatus.Tests` — normalizer, merger, endpoint tests
- `flight-status-ui` — plain HTML/CSS/JS demo
- `spec.md` — design contracts
