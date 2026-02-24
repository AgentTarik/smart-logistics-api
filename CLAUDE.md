# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project Overview

Smart Logistics & Delivery API — a backend that manages fleets, optimizes delivery routes, tracks deliveries in real time, and handles dispatch logic. Built with .NET 8 / C# 12 following Clean Architecture.

## Tech Stack

- **Runtime/Language:** .NET 8 / C# 12
- **API:** ASP.NET Core (Controllers + Minimal APIs)
- **ORM:** Entity Framework Core (PostgreSQL + PostGIS via NetTopologySuite) + Dapper for read-heavy queries
- **Cache:** Redis (StackExchange.Redis) — geospatial driver locations, distributed locking
- **Messaging:** Kafka (Confluent.Kafka) — order and delivery event topics
- **Auth:** JWT Bearer Tokens, three roles: Admin, Driver, Merchant
- **Validation:** FluentValidation with MediatR pipeline behavior
- **CQRS:** MediatR (commands/queries/domain events)
- **Resilience:** Polly (retry with exponential backoff, circuit breaker on external HTTP calls)
- **Logging:** Serilog (console + rolling file)
- **Testing:** xUnit, Moq, FluentAssertions, Testcontainers (PostgreSQL), WebApplicationFactory

## Build & Run Commands

```bash
# Restore and build
dotnet restore
dotnet build

# Run the API (from solution root)
dotnet run --project src/SmartLogistics.API

# Start infrastructure (PostgreSQL+PostGIS, Redis, Kafka)
docker-compose up -d

# Full stack with API container
docker-compose up --build

# Run all tests
dotnet test --verbosity normal

# Run only unit tests
dotnet test tests/SmartLogistics.UnitTests --verbosity normal

# Run only integration tests
dotnet test tests/SmartLogistics.IntegrationTests --verbosity normal

# EF Core migrations (from solution root)
dotnet ef migrations add <MigrationName> \
  --project src/SmartLogistics.Infrastructure \
  --startup-project src/SmartLogistics.API

dotnet ef database update \
  --project src/SmartLogistics.Infrastructure \
  --startup-project src/SmartLogistics.API
```

## Architecture (Clean Architecture)

```
src/
  SmartLogistics.Domain/           # Zero dependencies. Entities, enums, value objects, domain events, repository interfaces.
  SmartLogistics.Application/      # Depends on Domain. MediatR handlers (CQRS), DTOs, FluentValidation validators, services (RoutingService, LoadOptimizer, DriverStatsService).
  SmartLogistics.Infrastructure/   # Depends on Application. EF Core DbContext, repository implementations, Redis caching, Kafka producer/consumer, external services (geocoding), JWT service, DispatchEngine (BackgroundService).
  SmartLogistics.API/              # Depends on Infrastructure + Application. Controllers, middleware (ExceptionMiddleware), DI registration, Serilog/Swagger/JWT/Polly config.
tests/
  SmartLogistics.UnitTests/        # References Domain + Application. Algorithm and service tests.
  SmartLogistics.IntegrationTests/ # References API. WebApplicationFactory + Testcontainers tests.
```

**Dependency rule:** Domain depends on nothing. Application depends on Domain. Infrastructure depends on Application. API depends on all. Tests reference what they need.

## Key Patterns

- **CQRS via MediatR:** Commands and queries are in `Application/Features/{Entity}/Commands/` and `Queries/`. Each file contains both the record (command/query) and its handler.
- **Generic Repository:** `IRepository<T>` defined in Domain, implemented in `Infrastructure/Repositories/Repository.cs`. Registered as `AddScoped(typeof(IRepository<>), typeof(Repository<>))`.
- **ValidationBehavior:** MediatR pipeline behavior in `Application/Behaviors/ValidationBehavior.cs` auto-runs FluentValidation validators before handlers.
- **Domain Events:** Defined in `Domain/Events/` as `INotification` records, published via MediatR, handled in `Application/Features/{Entity}/EventHandlers/`. Events also forward to Kafka via `IEventPublisher`.
- **Order State Machine:** `Order.TransitionTo()` enforces valid state transitions: Created -> Assigned -> PickedUp -> InTransit -> Delivered/Failed. Cancelled allowed from Created/Assigned.
- **DispatchEngine:** BackgroundService in Infrastructure that polls every 5s, matches pending orders to nearest available drivers using priority queue ordering (by OrderPriority then CreatedAt) and geospatial proximity. Uses Redis distributed locks to prevent double-dispatch.
- **Exception Middleware:** `ExceptionMiddleware` maps ValidationException->400, KeyNotFoundException->404, unhandled->500.

## Domain Entities

Core entities (all extend `BaseEntity` with Id, CreatedAt, UpdatedAt): Driver, Merchant, Order, Delivery, Zone, ZoneConnection, User. PostGIS geometry types (Point, Polygon) used for locations and zone boundaries via NetTopologySuite.

## Key Infrastructure Services

- **DriverLocationCache:** Redis GEOADD/GEOSEARCH for real-time driver proximity queries
- **RedisCacheService:** Generic cache + distributed locking (AcquireLockAsync/ReleaseLockAsync)
- **KafkaProducer/OrderEventsConsumer:** Topics: `order-events`, `delivery-events`
- **GeocodingService:** External HTTP client with Polly retry (3x exponential backoff) + circuit breaker (opens after 5 failures for 30s)
- **JwtService:** Token generation with role claims + DriverId/MerchantId claims

## API Route Convention

All routes follow `api/v1/[controller]`. Auth at `/api/v1/auth/{register,login}`. Health check at `/api/health` and `/health`.
