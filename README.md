# DentalClinic.Appointments

A learning-oriented sample project that implements the **CQRS** pattern with
.NET 10, MediatR, FluentValidation, Entity Framework Core and Azure Service Bus,
keeping the write side (commands) and the read side (queries) clearly separated.

## Architecture

```
                    Write side                              Read side
  +----------------------------------------+     +-----------------------------+
  | Api (HTTP)                             |     | Worker (Projections.Worker) |
  |  Command handlers                      |     |  IntegrationEventDispatcher |
  |  FluentValidation                      |     |  IAppointmentProjector      |
  |  Domain model                          |     |  IInbox (idempotency)       |
  |  EF InMemory (write store)             |     |                             |
  |  Outbox table                          |     |  EF Postgres (read model)   |
  +------------------+---------------------+     +--------------+--------------+
                     |                                      |
                     +----------> appointment-events -------+
                                 (Service Bus topic)
```

- **Write side (API):** commands (`Schedule *`, `Reschedule *`, `Cancel *`)
  enforce domain rules (appointment overlap, validation), persist through an
  in-memory repository and enqueue an integration event in the **Outbox** table
  within the same unit of work (*transactional outbox*).
- **Service Bus:** the `OutboxProcessor` publishes pending messages to a topic
  with duplicate detection (10-minute window). Sessions guarantee ordered
  processing per appointment (`SessionId = AggregateId`).
- **Read side (worker):** subscribes to the topic, deserializes each event by
  its **stable key** (e.g. `appointment.scheduled.v1`) and updates the
  PostgreSQL read model. The **Inbox** records processed `EventId`s so
  at-least-once consumption stays idempotent.
- **Queries (API):** `GET /api/appointments` reads the paginated projection
  model, never the write store, returning DTOs shaped for the client view
  (appointments by dentist and date range).

## Architecture decisions (ADR)

### Write store backed by EF InMemory (educational requirement)

The write store is an in-memory `DbContext` **by choice of the project** to keep
the sample simple. Consequences:

- The outbox is real, but **not durable**: restarting the API after a message
  was queued discards the write store. The projection model is not lost; the
  outbox rows are wiped along with the in-memory store.
- In production the write side should use a real store (SQL Server / Postgres)
  so domain persistence and the outbox share one durable transaction. The rest
  of the architecture (topic, sessions, inbox, projections) is identical.

### Stable event keys

Messages travel with versioned semantic keys (`appointment.scheduled.v1`,
`appointment.rescheduled.v1`, `appointment.cancelled.v1`) instead of CLR type
names. This lets contracts evolve without coupling the consumer to the
publisher's assembly. The mapping lives in `IntegrationEventTypeMap`.

### View-oriented read model

The projection denormalizes fields derived from the aggregate (`Date` and
`DurationMinutes`) so clients do not have to compute them, and exposes a
composite index `(DentistId, Date)` aligned with the main agenda query.

### Pagination

`GET /api/appointments` returns a wrapper
`{ page, pageSize, totalCount, hasNextPage, items }` with defaults `page=1`,
`pageSize=20` (maximum 100). The read repository enforces the same bounds.

### Consumer inbox

Before projecting, the worker checks whether the `EventId` was already
processed and, if so, completes the message without side effects. The topic
deduplication window covers immediate retries; the inbox covers mid-term
duplicate deliveries.

## Prerequisites

- .NET 10 SDK (local runs only)
- Docker Compose (Docker image runs)
- An Azure Service Bus namespace with the following resources already created:
  - topic `appointment-events` with **duplicate detection** enabled
    (deduplication window, e.g. 10 minutes)
  - subscription `projection-appointments` with **sessions enabled**
- A `Send`/`Listen` SAS policy for the publisher (API) and, ideally, a
  dedicated `Listen`-only policy for the worker.
  Real keys live in your `.env` file and are never committed.

## Running with Docker

Copy `.env.example` to `.env` and fill in the keys:

```bash
cp .env.example .env
```

Then build and start the whole stack (Postgres, API, worker):

```bash
docker compose up -d --build
```

The API is available at http://localhost:8080.

- **Resetting the read model** (schema changes): `docker compose down -v` and
  bring it back up. `EnsureCreated` never alters existing tables, so a clean
  volume is required when projections or the inbox change shape.
- Container started in the foreground with logs:
  `docker compose up --build` or `docker compose logs -f`.

## Running locally

Start a local PostgreSQL instance (reuses the Compose service, exposing port
`5432`):

```bash
docker compose up -d postgres
```

Set the environment variables. PowerShell example:

```powershell
$env:ConnectionStrings__ReadDatabase = "Host=localhost;Database=dental_read;Username=dental;Password=dental"
$env:AzureServiceBus__ConnectionString = "Endpoint=sb://.../;SharedAccessKeyName=...;SharedAccessKey=..."
$env:AzureServiceBus__TopicName = "appointment-events"
$env:AzureServiceBus__SubscriptionName = "projection-appointments"
```

1. Run the worker (needs Service Bus and Postgres; it consumes the topic and
   updates the read model):

   ```bash
   dotnet run --project DentalClinic.Appointments.Projections.Worker
   ```

2. In another terminal, run the API (takes `http://localhost:5110` by default):

   ```bash
   dotnet run --project DentalClinic.Appointments.Api
   ```

Notes for local development:

- The API only registers the outbox publisher when
  `AzureServiceBus__ConnectionString` and `AzureServiceBus__TopicName` are set;
  without them, commands work but nothing is published.
- The write store is in-memory, so **restarting the API loses created
  appointments**. Keep the API running while you create data and wait ~5 s for
  each outbox poll before checking the read side.
- Set `ReadDataProvider=InMemory` to run the API without PostgreSQL — mainly
  useful in tests (the `WebApplicationFactory` uses it); projections still flow
  through the worker, which always needs Postgres.

## Endpoints

| Method | Route | Description |
| --- | --- | --- |
| `POST` | `/api/appointments` | Schedules an appointment (201 + `Location` header) |
| `PUT` | `/api/appointments/{id}/reschedule` | Reschedules an appointment |
| `DELETE` | `/api/appointments/{id}` | Cancels an appointment |
| `GET` | `/api/appointments/{id}` | Returns one appointment from the read model |
| `GET` | `/api/appointments` | Paginated list (filters `dentistId`, `from`, `to`; pagination `page`, `pageSize`) |

## Tests

```bash
dotnet test
```

- **UnitTests:** domain, validation, outbox, event dispatcher, projector,
  inbox and in-memory read repository.
- **IntegrationTests:** full HTTP round-trip against a `WebApplicationFactory`
  with the Service Bus and read model simulated, including the reprojection
  flow and pagination.

## Project structure

```
DentalClinic.Appointments.Api                 # Minimal API endpoints + hosting
DentalClinic.Appointments.Application         # Queries, commands, handlers, outbox contracts
DentalClinic.Appointments.Domain              # Appointment aggregate and business rules
DentalClinic.Appointments.Infrastructure      # Write repositories, EF InMemory, OutboxProcessor
DentalClinic.Appointments.ReadModel           # Projections, read repository, Inbox
DentalClinic.Appointments.Projections.Worker  # Service Bus consumer
DentalClinic.Appointments.UnitTests           # Unit tests
DentalClinic.Appointments.IntegrationTests    # HTTP integration tests
```