## first prompt
I am designing a flight status API. Two providers: AeroTrack(full detail - status, scheduled/actual times, terminal,gate,delay reason, lastUpdatedUtc) and QuickFlight(minimal- status,scheduled times,lastUpdatedUtc). I need to normalise both into a unified enum: OnTime/Delayed/Cancelled/Diverted/Unknown, using the rules:
Status Rule
OnTime Departure or arrival within 15 minutes of schedule
Delayed Departure or arrival pushed beyond 15 minutes
Cancelled Flight will not operate
Diverted Flight landed at a different airport
Unknown No usable status returned by either provider

help me draft C# record/class definitions for the raw provider DTOs and the unified FlightStatusResult model, plus the IFlightStatusProvider interface signature. Dont write implementation yet, just the contracts

##second prompt

Need to change the layout structure to use as shown below:

flight-status/
├── README.md # setup, run steps, assumptions
├── spec.md # data models and interface contracts
├── FlightStatus.Api/
├── FlightStatus.Tests/
├── <case-name>-ui/
├── prompts.md 
└── reflection.md 

provider Interface - use option A
Use Normalizer conteact as seperate from fetching

perform the refinement

##3rd prompt

In FlightStatus.Api/models, create the DTOs and unified model exactly as efined in spec.md. use c# records, Add the IFlightStatusProvider interface with the method already defined in spec.md file

##4th prompt

Implement AeroTractStubProvider: IFlightStatusProvider. it should be deterministic based on flightNumber/date input(e.g. hash or last digit of flight number) and return varied scnarios: OnTime, Delayed, Cancelled, Diverted, and one 'no data' case. Include terminal, gate, delay reason, actual times, lastUpdatedUtc. No randomness - same input always same output

##5th prompt

Now implement QuickFlightStubProvider: IFlightStatusProvider the same way but only status + schedueld times + lastUpdatedUtc

##6th prompt

Implement a statusNormaliser class that converts each provider's raw status vocabularly into the unified enum per the already given rules. then implement a FlightStatusMerger that takes optional results from both providers and applied: prefer later lastUpdatedUtc if both present, use whichever responded if only one does, return unknown with a message if neither did. keep these as seperate, independent testable classes - dont inline the logic into endpoint

##7th prompt

wire up GET/flights/status?flightNumber={code}&date={yyyy-MM-dd} in Program.cs using minimal API. inject both providers via DI as IEnumerable or named services, call both, pass to the merger, return the unified result. return 400 with a clear error body if flightNumber or date is missing or date is malformed

##8th prompt

write xUnit tests for StatusNormaliser covering every mapping rule in spec.md, including edge cases at 15-minute boundary

##9th prompt

next write test cases for FlightStatusMerger covering - both respond, only one responds, neither responds and a tie. Also integration-style tests for /flights/status endpoint using WebApplicationFactory, covering happypath, missing flight number, midding date, malformed date returning 400

##10th prompt

Build a sinle page app in flight-status/flight-status-ui using plain HTML/CSS. A form with flight number + date inputs. On submit, call Get method on localhost /flights/status/ Render a result card: status badge colour-coded 9green = OnTime, amber = Delayed, red = cancelled/diverted, grey = unknown). show gate/terminal/delay reason only if present in response. show clear error message block if fetch fails or returns 400. if CORS is not enabled, please do it in program file