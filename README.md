# Backend Engineer Challenge - Rockets

## Overview

This project is a small ASP.NET Core API that consumes rocket messages and exposes the current state of each rocket through a REST API.

The main challenge in the assignment is not persistence, but handling message streams correctly when messages:

- arrive out of order
- are delivered more than once
- are redelivered late

The solution treats each rocket as its own message stream and projects a current state from that stream.

## Tech Stack

- .NET 10
- ASP.NET Core minimal API
- In-memory state storage

## Endpoints

### `POST /messages`

Accepts one rocket message in the assignment envelope format.

Example:

```json
{
  "metadata": {
    "channel": "193270a9-c9cf-404a-8f83-838e71d9ae67",
    "messageNumber": 1,
    "messageTime": "2022-02-02T19:39:05.86337+01:00",
    "messageType": "RocketLaunched"
  },
  "message": {
    "type": "Falcon-9",
    "launchSpeed": 500,
    "mission": "ARTEMIS"
  }
}
```

### `GET /rocket/{id}`

Returns the current projected state for a single rocket.

### `GET /rockets`

Returns all known rockets.

The current implementation sorts rockets by `Id`.

### `GET /health-check`

Simple health endpoint.

## Supported Message Types

- `RocketLaunched`
- `RocketSpeedIncreased`
- `RocketSpeedDecreased`
- `RocketMissionChanged`
- `RocketExploded`

## How The Ordering Works

Each rocket is treated as its own stream, keyed by `metadata.channel`.

For each rocket, the service keeps:

- the current projected `RocketState`
- the next expected `messageNumber`
- a buffer of pending out-of-order messages

When a message arrives:

1. If it is older than the next expected message number, it is ignored.
2. If it is ahead of the next expected message number, it is buffered.
3. If it is exactly the next expected message number, it is applied immediately.
4. After a message is applied, the service tries to drain any buffered follow-up messages in sequence.

This makes duplicate delivery, late redelivery, and out-of-order delivery deterministic.

## Design Choices

### In-memory storage

I chose in-memory storage instead of a database because the timebox was limited and the core challenge was message ordering, deduplication, and projection logic.

This is a deliberate simplification.

Tradeoff:

- all state is lost if the service restarts

### Per-rocket aggregate

Instead of treating the latest rocket state as the only source of truth, the solution keeps a per-rocket stream aggregate internally.

That aggregate is responsible for:

- message ordering
- duplicate suppression
- buffering future messages
- terminal state handling

### Separation of responsibilities

The code is split into smaller focused parts:

- `MessageParser` parses the envelope into typed messages
- `MessageHandler` orchestrates message processing
- `RocketStreamAggregate` handles ordering and buffering
- `RocketStateProjector` applies typed messages to state
- `RocketStateService` owns the in-memory store

This keeps the tricky logic isolated and easier to reason about.

## Verification

The solution was verified in two ways:

### Custom test harness

A lightweight test project verifies the most important message-handling behavior:

- out-of-order delivery
- duplicate delivery
- late redelivery
- messages arriving before launch
- terminal explosion behavior

Run it with:

```bash
dotnet run --project tests/Rockets.Api.Tests/Rockets.Api.Tests.csproj
```

### HTTP tests

The file `src/Rockets.Api/Rockets.Api.http` contains request examples for:

- posting messages
- checking health
- fetching a single rocket
- fetching all rockets

## Running The Project

Start the API:

```bash
dotnet run --project src/Rockets.Api/Rockets.Api.csproj
```

Build the solution:

```bash
dotnet build Rockets.sln
```

## What I Would Change In A Production Version

- persist state in a database
- add standard test framework coverage for HTTP integration tests
- add stronger input validation and clearer error handling
- add observability such as logging, metrics, and tracing

## Notes On AI Usage

AI was used as a development accelerator for:

- scaffolding
- architecture sparring
- formatting and documentation help
- implementation of Aggregate

I kept ownership of the core design decisions, especially around:

- message ordering
- duplicate handling
- state projection
- test verification

Where AI suggested solutions that were too broad or did not fit the assignment well, the implementation was narrowed back down to match the timebox and the actual problem.
