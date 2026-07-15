# Reflection

## What went well

- Separating providers made each piece small and easy to test in isolation.
- Deterministic stubs make demos and tests predictable without mocking HTTP.
- Plain HTML/JS for the UI keeps setup minimal and avoids a separate frontend build step.

## What I would improve with more time

1. Provider registration — register providers via `IEnumerable<IFlightStatusProvider>` (or a small registry) so adding a third provider does not require changing `Program.cs`.
2. Richer normalization tests — add explicit coverage for Diverted, arrival-leg timing, and vocabulary variants from both providers; today the core rules are covered but not every enum path.
3. Merge detail preservation — when the winning provider is QuickFlight, back-fill AeroTrack-only fields (gate, terminal, delay reason) from the loser if timestamps are compatible, so support agents always see available detail.
4. OpenAPI + typed client — expose Swagger and generate a small TS client for the UI instead of hand-mapping JSON field names.
5. Operational hardening — structured logging, health check endpoint, configurable API base URL in the UI, and retry/timeout policies around provider calls (even for stubs, to model real integrations).


## AI tool notes

AI was used for contract design (`spec.md`), implementation scaffolding, and test case drafting. My judgement was applied on merge tie-breaking, the 15-minute boundary semantics (inclusive vs exclusive), and keeping normalization out of providers per the challenge's separation of concerns.
