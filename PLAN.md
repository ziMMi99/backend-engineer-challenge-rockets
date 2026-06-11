# Rocket Challenge Plan

This is the working plan I’ll use to build the challenge solution. I want to keep ownership of the domain logic, ordering rules, deduplication behavior, and test assertions, while using AI only to speed up scaffolding and wording where useful.

## What I Need To Figure Out

1. Read the prompt carefully and rewrite the rules in plain English.
2. Decide the domain model:
   - what a rocket state contains
   - what each message type changes
   - what the terminal states are
3. Decide the ordering rule for out-of-order messages.
4. Decide the duplicate strategy.
5. Decide the API shape and sorting for `GET /rockets`.

## What I Will Implement

1. The message handling logic.
2. The state projection logic.
3. The API endpoints and response shape.
4. The tests for the tricky parts.
5. The README in my own words.

## Tests To Prioritize

1. Out-of-order delivery.
2. Duplicate delivery.
3. Late redelivery.
4. State transitions.

## Interview Preparation

I should be ready to explain:

1. What problem the service solves.
2. What tradeoffs I made.
3. Why I chose the domain model I used.
4. How the ordering and deduplication rules work.
5. What I would improve next in a production version.

## Good Interview Framing

- I used AI to accelerate scaffolding and drafting, but I owned the problem analysis, the domain logic, and the tests.
- I verified the tricky parts myself, especially ordering and deduplication.
- I kept the solution small on purpose because the timebox was six hours.

## What I Should Be Ready To Defend

1. Why I chose in-memory storage.
2. How out-of-order messages are handled.
3. How duplicates are ignored safely.
4. Why the rocket state model is shaped the way it is.
5. What happens if the service restarts.
6. What I would change for a production version.

