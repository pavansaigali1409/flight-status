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

