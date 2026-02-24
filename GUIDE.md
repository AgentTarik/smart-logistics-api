# 🚚 Smart Logistics & Delivery API — Complete Build Guide

## Project Overview

You are building a backend API that manages fleets, optimizes delivery routes, tracks deliveries in real time, and handles dispatch logic. This project will teach you the core .NET ecosystem used in enterprise environments.

### Tech Stack

| Layer | Technology |
|---|---|
| Runtime | .NET 8 |
| Language | C# 12 |
| API | ASP.NET Core (Controllers + Minimal APIs) |
| ORM | Entity Framework Core + Dapper |
| Database | PostgreSQL + PostGIS |
| Cache | Redis |
| Messaging | Kafka (via Confluent.Kafka) |
| Auth | JWT Bearer Tokens |
| Validation | FluentValidation |
| CQRS | MediatR |
| Resilience | Polly |
| Logging | Serilog |
| Testing | xUnit, Moq, Testcontainers |
| Containers | Docker + docker-compose |
| CI | GitHub Actions |
| Docs | Swagger / OpenAPI |

### Project Architecture (Clean Architecture)

```
SmartLogistics/
├── src/
│   ├── SmartLogistics.Domain/            # Entities, value objects, domain events, interfaces
│   ├── SmartLogistics.Application/       # Use cases, MediatR handlers, DTOs, validators
│   ├── SmartLogistics.Infrastructure/    # EF Core, Redis, Kafka, external APIs
│   └── SmartLogistics.API/              # Controllers, middleware, startup config
├── tests/
│   ├── SmartLogistics.UnitTests/
│   └── SmartLogistics.IntegrationTests/
├── docker-compose.yml
└── SmartLogistics.sln
```

---

## ✅ Checkpoint 1 — Solution Scaffolding & Project Structure

**Goal:** Have a running empty API with Clean Architecture, Swagger, Serilog, and Docker.

### Step 1.1 — Create the solution and projects

```bash
# Create the solution folder
mkdir SmartLogistics && cd SmartLogistics

# Create the solution file
dotnet new sln -n SmartLogistics

# Create the projects
dotnet new classlib -n SmartLogistics.Domain -o src/SmartLogistics.Domain
dotnet new classlib -n SmartLogistics.Application -o src/SmartLogistics.Application
dotnet new classlib -n SmartLogistics.Infrastructure -o src/SmartLogistics.Infrastructure
dotnet new webapi -n SmartLogistics.API -o src/SmartLogistics.API --use-controllers

# Create test projects
dotnet new xunit -n SmartLogistics.UnitTests -o tests/SmartLogistics.UnitTests
dotnet new xunit -n SmartLogistics.IntegrationTests -o tests/SmartLogistics.IntegrationTests

# Add all projects to the solution
dotnet sln add src/SmartLogistics.Domain/SmartLogistics.Domain.csproj
dotnet sln add src/SmartLogistics.Application/SmartLogistics.Application.csproj
dotnet sln add src/SmartLogistics.Infrastructure/SmartLogistics.Infrastructure.csproj
dotnet sln add src/SmartLogistics.API/SmartLogistics.API.csproj
dotnet sln add tests/SmartLogistics.UnitTests/SmartLogistics.UnitTests.csproj
dotnet sln add tests/SmartLogistics.IntegrationTests/SmartLogistics.IntegrationTests.csproj
```

### Step 1.2 — Set up project references (dependency rule)

The dependency rule of Clean Architecture: **Domain depends on nothing. Application depends on Domain. Infrastructure depends on Application. API depends on all.**

```bash
# Application references Domain
dotnet add src/SmartLogistics.Application reference src/SmartLogistics.Domain

# Infrastructure references Application (and transitively Domain)
dotnet add src/SmartLogistics.Infrastructure reference src/SmartLogistics.Application

# API references Infrastructure and Application
dotnet add src/SmartLogistics.API reference src/SmartLogistics.Infrastructure
dotnet add src/SmartLogistics.API reference src/SmartLogistics.Application

# Test projects reference what they need
dotnet add tests/SmartLogistics.UnitTests reference src/SmartLogistics.Domain
dotnet add tests/SmartLogistics.UnitTests reference src/SmartLogistics.Application
dotnet add tests/SmartLogistics.IntegrationTests reference src/SmartLogistics.API
```

### Step 1.3 — Install NuGet packages

```bash
# --- Domain (keep it lean, almost no packages) ---
# (none for now)

# --- Application ---
dotnet add src/SmartLogistics.Application package MediatR
dotnet add src/SmartLogistics.Application package FluentValidation
dotnet add src/SmartLogistics.Application package FluentValidation.DependencyInjectionExtensions

# --- Infrastructure ---
dotnet add src/SmartLogistics.Infrastructure package Npgsql.EntityFrameworkCore.PostgreSQL
dotnet add src/SmartLogistics.Infrastructure package Npgsql.EntityFrameworkCore.PostgreSQL.NetTopologySuite
dotnet add src/SmartLogistics.Infrastructure package Microsoft.EntityFrameworkCore.Tools
dotnet add src/SmartLogistics.Infrastructure package Dapper
dotnet add src/SmartLogistics.Infrastructure package StackExchange.Redis
dotnet add src/SmartLogistics.Infrastructure package Confluent.Kafka
dotnet add src/SmartLogistics.Infrastructure package Microsoft.Extensions.Http.Polly
dotnet add src/SmartLogistics.Infrastructure package Polly

# --- API ---
dotnet add src/SmartLogistics.API package Serilog.AspNetCore
dotnet add src/SmartLogistics.API package Serilog.Sinks.Console
dotnet add src/SmartLogistics.API package Serilog.Sinks.File
dotnet add src/SmartLogistics.API package Swashbuckle.AspNetCore
dotnet add src/SmartLogistics.API package Microsoft.AspNetCore.Authentication.JwtBearer
dotnet add src/SmartLogistics.API package AspNetCoreRateLimit

# --- Tests ---
dotnet add tests/SmartLogistics.UnitTests package Moq
dotnet add tests/SmartLogistics.UnitTests package FluentAssertions
dotnet add tests/SmartLogistics.IntegrationTests package Microsoft.AspNetCore.Mvc.Testing
dotnet add tests/SmartLogistics.IntegrationTests package Testcontainers.PostgreSql
dotnet add tests/SmartLogistics.IntegrationTests package FluentAssertions
```

### Step 1.4 — Set up Serilog in `Program.cs`

Open `src/SmartLogistics.API/Program.cs` and replace with:

```csharp
using Serilog;

Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .WriteTo.File("logs/log-.txt", rollingInterval: RollingInterval.Day)
    .Enrich.FromLogContext()
    .CreateLogger();

try
{
    Log.Information("Starting SmartLogistics API");

    var builder = WebApplication.CreateBuilder(args);
    builder.Host.UseSerilog();

    // Add services
    builder.Services.AddControllers();
    builder.Services.AddEndpointsApiExplorer();
    builder.Services.AddSwaggerGen();

    var app = builder.Build();

    // Middleware pipeline
    app.UseSerilogRequestLogging();

    if (app.Environment.IsDevelopment())
    {
        app.UseSwagger();
        app.UseSwaggerUI();
    }

    app.UseHttpsRedirection();
    app.UseAuthorization();
    app.MapControllers();

    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Application terminated unexpectedly");
}
finally
{
    Log.CloseAndFlush();
}
```

### Step 1.5 — Create a Health Check controller

Create `src/SmartLogistics.API/Controllers/HealthController.cs`:

```csharp
using Microsoft.AspNetCore.Mvc;

namespace SmartLogistics.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class HealthController : ControllerBase
{
    [HttpGet]
    public IActionResult Get() => Ok(new { Status = "Healthy", Timestamp = DateTime.UtcNow });
}
```

### Step 1.6 — Create docker-compose.yml

Create `docker-compose.yml` at the solution root:

```yaml
version: '3.8'

services:
  postgres:
    image: postgis/postgis:16-3.4
    container_name: smartlogistics-db
    environment:
      POSTGRES_USER: smartlogistics
      POSTGRES_PASSWORD: devpassword123
      POSTGRES_DB: smartlogistics
    ports:
      - "5432:5432"
    volumes:
      - postgres_data:/var/lib/postgresql/data

  redis:
    image: redis:7-alpine
    container_name: smartlogistics-redis
    ports:
      - "6379:6379"

  kafka:
    image: confluentinc/cp-kafka:7.5.0
    container_name: smartlogistics-kafka
    environment:
      KAFKA_NODE_ID: 1
      KAFKA_LISTENER_SECURITY_PROTOCOL_MAP: CONTROLLER:PLAINTEXT,PLAINTEXT:PLAINTEXT,HOST:PLAINTEXT
      KAFKA_ADVERTISED_LISTENERS: PLAINTEXT://kafka:29092,HOST://localhost:9092
      KAFKA_LISTENERS: PLAINTEXT://0.0.0.0:29092,CONTROLLER://0.0.0.0:29093,HOST://0.0.0.0:9092
      KAFKA_CONTROLLER_LISTENER_NAMES: CONTROLLER
      KAFKA_CONTROLLER_QUORUM_VOTERS: 1@kafka:29093
      KAFKA_PROCESS_ROLES: broker,controller
      KAFKA_OFFSETS_TOPIC_REPLICATION_FACTOR: 1
      CLUSTER_ID: "smartlogistics-cluster-001"
    ports:
      - "9092:9092"

volumes:
  postgres_data:
```

### Step 1.7 — Configure appsettings.json

Update `src/SmartLogistics.API/appsettings.Development.json`:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Port=5432;Database=smartlogistics;Username=smartlogistics;Password=devpassword123",
    "Redis": "localhost:6379"
  },
  "Kafka": {
    "BootstrapServers": "localhost:9092",
    "GroupId": "smartlogistics-api"
  },
  "JwtSettings": {
    "Secret": "YourSuperSecretKeyThatIsAtLeast32CharactersLong!!",
    "Issuer": "SmartLogisticsAPI",
    "Audience": "SmartLogisticsClients",
    "ExpirationInMinutes": 60
  },
  "Logging": {
    "LogLevel": {
      "Default": "Information"
    }
  }
}
```

### Step 1.8 — Verify everything works

```bash
# Start infrastructure
docker-compose up -d

# Run the API
cd src/SmartLogistics.API
dotnet run

# Open browser: https://localhost:5001/swagger
# Test: GET /api/health should return 200
```

**✅ Checkpoint 1 complete when:** You can hit `/api/health` on Swagger, Serilog prints logs to console, and docker-compose runs Postgres, Redis, and Kafka.

---

## ✅ Checkpoint 2 — Domain Entities & EF Core Setup

**Goal:** Define your core domain models and get EF Core connected to PostgreSQL with PostGIS.

### Step 2.1 — Create domain entities

Create these files inside `src/SmartLogistics.Domain/Entities/`:

**`src/SmartLogistics.Domain/Entities/BaseEntity.cs`**
```csharp
namespace SmartLogistics.Domain.Entities;

public abstract class BaseEntity
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}
```

**`src/SmartLogistics.Domain/Entities/Driver.cs`**
```csharp
using NetTopologySuite.Geometries;

namespace SmartLogistics.Domain.Entities;

public class Driver : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string PhoneNumber { get; set; } = string.Empty;
    public string LicensePlate { get; set; } = string.Empty;
    public DriverStatus Status { get; set; } = DriverStatus.Offline;
    public Point? CurrentLocation { get; set; }  // PostGIS geometry
    public double MaxCargoWeightKg { get; set; }
    public double MaxCargoVolumeM3 { get; set; }

    // Navigation properties
    public ICollection<Delivery> Deliveries { get; set; } = new List<Delivery>();
}

public enum DriverStatus
{
    Offline,
    Available,
    OnDelivery,
    OnBreak
}
```

**`src/SmartLogistics.Domain/Entities/Merchant.cs`**
```csharp
using NetTopologySuite.Geometries;

namespace SmartLogistics.Domain.Entities;

public class Merchant : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string ApiKey { get; set; } = Guid.NewGuid().ToString("N");
    public string Address { get; set; } = string.Empty;
    public Point Location { get; set; } = null!;  // PostGIS geometry

    // Navigation properties
    public ICollection<Order> Orders { get; set; } = new List<Order>();
}
```

**`src/SmartLogistics.Domain/Entities/Order.cs`**
```csharp
using NetTopologySuite.Geometries;

namespace SmartLogistics.Domain.Entities;

public class Order : BaseEntity
{
    public Guid MerchantId { get; set; }
    public string CustomerName { get; set; } = string.Empty;
    public string CustomerPhone { get; set; } = string.Empty;
    public string PickupAddress { get; set; } = string.Empty;
    public Point PickupLocation { get; set; } = null!;
    public string DeliveryAddress { get; set; } = string.Empty;
    public Point DeliveryLocation { get; set; } = null!;
    public double WeightKg { get; set; }
    public double VolumeM3 { get; set; }
    public OrderPriority Priority { get; set; } = OrderPriority.Normal;
    public OrderStatus Status { get; set; } = OrderStatus.Created;
    public DateTime? Deadline { get; set; }

    // Navigation properties
    public Merchant Merchant { get; set; } = null!;
    public Delivery? Delivery { get; set; }
}

public enum OrderStatus
{
    Created,
    Assigned,
    PickedUp,
    InTransit,
    Delivered,
    Failed,
    Cancelled
}

public enum OrderPriority
{
    Low,
    Normal,
    High,
    Urgent
}
```

**`src/SmartLogistics.Domain/Entities/Delivery.cs`**
```csharp
namespace SmartLogistics.Domain.Entities;

public class Delivery : BaseEntity
{
    public Guid OrderId { get; set; }
    public Guid DriverId { get; set; }
    public DeliveryStatus Status { get; set; } = DeliveryStatus.Pending;
    public DateTime? PickedUpAt { get; set; }
    public DateTime? DeliveredAt { get; set; }
    public double? DistanceKm { get; set; }
    public string? Notes { get; set; }

    // Navigation properties
    public Order Order { get; set; } = null!;
    public Driver Driver { get; set; } = null!;
}

public enum DeliveryStatus
{
    Pending,
    DriverEnRoute,
    PickedUp,
    InTransit,
    Delivered,
    Failed
}
```

**`src/SmartLogistics.Domain/Entities/Zone.cs`**
```csharp
using NetTopologySuite.Geometries;

namespace SmartLogistics.Domain.Entities;

public class Zone : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public Polygon Boundary { get; set; } = null!;  // PostGIS polygon
    public double BaseDeliveryCost { get; set; }

    // Navigation properties
    public ICollection<ZoneConnection> OutgoingConnections { get; set; } = new List<ZoneConnection>();
    public ICollection<ZoneConnection> IncomingConnections { get; set; } = new List<ZoneConnection>();
}
```

**`src/SmartLogistics.Domain/Entities/ZoneConnection.cs`**
```csharp
namespace SmartLogistics.Domain.Entities;

/// <summary>
/// Represents a weighted, directed edge between two zones.
/// This is the graph edge for routing algorithms.
/// </summary>
public class ZoneConnection : BaseEntity
{
    public Guid FromZoneId { get; set; }
    public Guid ToZoneId { get; set; }
    public double DistanceKm { get; set; }
    public double EstimatedTimeMinutes { get; set; }
    public double TrafficMultiplier { get; set; } = 1.0;

    // Navigation properties
    public Zone FromZone { get; set; } = null!;
    public Zone ToZone { get; set; } = null!;
}
```

### Step 2.2 — Add NetTopologySuite to the Domain project

```bash
dotnet add src/SmartLogistics.Domain package NetTopologySuite
```

### Step 2.3 — Create the DbContext in Infrastructure

Create `src/SmartLogistics.Infrastructure/Data/AppDbContext.cs`:

```csharp
using Microsoft.EntityFrameworkCore;
using SmartLogistics.Domain.Entities;

namespace SmartLogistics.Infrastructure.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<Driver> Drivers => Set<Driver>();
    public DbSet<Merchant> Merchants => Set<Merchant>();
    public DbSet<Order> Orders => Set<Order>();
    public DbSet<Delivery> Deliveries => Set<Delivery>();
    public DbSet<Zone> Zones => Set<Zone>();
    public DbSet<ZoneConnection> ZoneConnections => Set<ZoneConnection>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Driver
        modelBuilder.Entity<Driver>(e =>
        {
            e.HasKey(d => d.Id);
            e.Property(d => d.Name).HasMaxLength(200).IsRequired();
            e.Property(d => d.Email).HasMaxLength(300).IsRequired();
            e.HasIndex(d => d.Email).IsUnique();
            e.Property(d => d.CurrentLocation).HasColumnType("geography (point)");
        });

        // Merchant
        modelBuilder.Entity<Merchant>(e =>
        {
            e.HasKey(m => m.Id);
            e.Property(m => m.Name).HasMaxLength(200).IsRequired();
            e.Property(m => m.ApiKey).HasMaxLength(64).IsRequired();
            e.HasIndex(m => m.ApiKey).IsUnique();
            e.Property(m => m.Location).HasColumnType("geography (point)");
        });

        // Order
        modelBuilder.Entity<Order>(e =>
        {
            e.HasKey(o => o.Id);
            e.Property(o => o.PickupLocation).HasColumnType("geography (point)");
            e.Property(o => o.DeliveryLocation).HasColumnType("geography (point)");
            e.HasOne(o => o.Merchant)
                .WithMany(m => m.Orders)
                .HasForeignKey(o => o.MerchantId)
                .OnDelete(DeleteBehavior.Restrict);
            e.HasOne(o => o.Delivery)
                .WithOne(d => d.Order)
                .HasForeignKey<Delivery>(d => d.OrderId);
        });

        // Delivery
        modelBuilder.Entity<Delivery>(e =>
        {
            e.HasKey(d => d.Id);
            e.HasOne(d => d.Driver)
                .WithMany(dr => dr.Deliveries)
                .HasForeignKey(d => d.DriverId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // Zone
        modelBuilder.Entity<Zone>(e =>
        {
            e.HasKey(z => z.Id);
            e.Property(z => z.Name).HasMaxLength(100).IsRequired();
            e.Property(z => z.Boundary).HasColumnType("geography (polygon)");
        });

        // ZoneConnection (graph edges)
        modelBuilder.Entity<ZoneConnection>(e =>
        {
            e.HasKey(zc => zc.Id);
            e.HasIndex(zc => new { zc.FromZoneId, zc.ToZoneId }).IsUnique();
            e.HasOne(zc => zc.FromZone)
                .WithMany(z => z.OutgoingConnections)
                .HasForeignKey(zc => zc.FromZoneId)
                .OnDelete(DeleteBehavior.Cascade);
            e.HasOne(zc => zc.ToZone)
                .WithMany(z => z.IncomingConnections)
                .HasForeignKey(zc => zc.ToZoneId)
                .OnDelete(DeleteBehavior.Restrict);
        });
    }

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        foreach (var entry in ChangeTracker.Entries<BaseEntity>())
        {
            if (entry.State == EntityState.Modified)
                entry.Entity.UpdatedAt = DateTime.UtcNow;
        }
        return base.SaveChangesAsync(cancellationToken);
    }
}
```

### Step 2.4 — Register the DbContext in Program.cs

Add to `Program.cs` in the services section:

```csharp
using Microsoft.EntityFrameworkCore;
using SmartLogistics.Infrastructure.Data;

// Add after builder.Services.AddControllers();
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(
        builder.Configuration.GetConnectionString("DefaultConnection"),
        x => x.UseNetTopologySuite()
    ));
```

### Step 2.5 — Create and run the first migration

```bash
# Install the EF tool if you haven't
dotnet tool install --global dotnet-ef

# Create the initial migration (run from solution root)
dotnet ef migrations add InitialCreate \
  --project src/SmartLogistics.Infrastructure \
  --startup-project src/SmartLogistics.API

# Apply migration
dotnet ef database update \
  --project src/SmartLogistics.Infrastructure \
  --startup-project src/SmartLogistics.API
```

**✅ Checkpoint 2 complete when:** The migration runs, tables appear in PostgreSQL, and you can verify them via a tool like `pgAdmin` or `psql`.

---

## ✅ Checkpoint 3 — CRUD Endpoints with Repository Pattern & MediatR

**Goal:** Create full CRUD for Drivers, Merchants, and Zones using the Repository pattern and MediatR CQRS.

### Step 3.1 — Define repository interfaces in Domain

Create `src/SmartLogistics.Domain/Interfaces/IRepository.cs`:

```csharp
using System.Linq.Expressions;
using SmartLogistics.Domain.Entities;

namespace SmartLogistics.Domain.Interfaces;

public interface IRepository<T> where T : BaseEntity
{
    Task<T?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<IReadOnlyList<T>> GetAllAsync(CancellationToken ct = default);
    Task<IReadOnlyList<T>> FindAsync(Expression<Func<T, bool>> predicate, CancellationToken ct = default);
    Task<T> AddAsync(T entity, CancellationToken ct = default);
    Task UpdateAsync(T entity, CancellationToken ct = default);
    Task DeleteAsync(T entity, CancellationToken ct = default);
}
```

Create `src/SmartLogistics.Domain/Interfaces/IUnitOfWork.cs`:

```csharp
namespace SmartLogistics.Domain.Interfaces;

public interface IUnitOfWork : IDisposable
{
    Task<int> SaveChangesAsync(CancellationToken ct = default);
}
```

### Step 3.2 — Implement the generic repository in Infrastructure

Create `src/SmartLogistics.Infrastructure/Repositories/Repository.cs`:

```csharp
using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using SmartLogistics.Domain.Entities;
using SmartLogistics.Domain.Interfaces;
using SmartLogistics.Infrastructure.Data;

namespace SmartLogistics.Infrastructure.Repositories;

public class Repository<T> : IRepository<T> where T : BaseEntity
{
    protected readonly AppDbContext _context;
    protected readonly DbSet<T> _dbSet;

    public Repository(AppDbContext context)
    {
        _context = context;
        _dbSet = context.Set<T>();
    }

    public async Task<T?> GetByIdAsync(Guid id, CancellationToken ct = default)
        => await _dbSet.FindAsync(new object[] { id }, ct);

    public async Task<IReadOnlyList<T>> GetAllAsync(CancellationToken ct = default)
        => await _dbSet.ToListAsync(ct);

    public async Task<IReadOnlyList<T>> FindAsync(Expression<Func<T, bool>> predicate, CancellationToken ct = default)
        => await _dbSet.Where(predicate).ToListAsync(ct);

    public async Task<T> AddAsync(T entity, CancellationToken ct = default)
    {
        await _dbSet.AddAsync(entity, ct);
        await _context.SaveChangesAsync(ct);
        return entity;
    }

    public async Task UpdateAsync(T entity, CancellationToken ct = default)
    {
        _dbSet.Update(entity);
        await _context.SaveChangesAsync(ct);
    }

    public async Task DeleteAsync(T entity, CancellationToken ct = default)
    {
        _dbSet.Remove(entity);
        await _context.SaveChangesAsync(ct);
    }
}
```

### Step 3.3 — Create DTOs in Application

Create `src/SmartLogistics.Application/DTOs/DriverDtos.cs`:

```csharp
namespace SmartLogistics.Application.DTOs;

public record CreateDriverRequest(
    string Name,
    string Email,
    string PhoneNumber,
    string LicensePlate,
    double MaxCargoWeightKg,
    double MaxCargoVolumeM3
);

public record UpdateDriverRequest(
    string Name,
    string PhoneNumber,
    string LicensePlate,
    double MaxCargoWeightKg,
    double MaxCargoVolumeM3
);

public record DriverResponse(
    Guid Id,
    string Name,
    string Email,
    string PhoneNumber,
    string LicensePlate,
    string Status,
    double? Latitude,
    double? Longitude,
    double MaxCargoWeightKg,
    double MaxCargoVolumeM3,
    DateTime CreatedAt
);

public record UpdateDriverLocationRequest(
    double Latitude,
    double Longitude
);
```

Create similar DTO files for Merchant and Zone. Follow the same pattern.

### Step 3.4 — Create MediatR commands and handlers for Driver

Create `src/SmartLogistics.Application/Features/Drivers/Commands/CreateDriver.cs`:

```csharp
using MediatR;
using SmartLogistics.Application.DTOs;
using SmartLogistics.Domain.Entities;
using SmartLogistics.Domain.Interfaces;

namespace SmartLogistics.Application.Features.Drivers.Commands;

// Command
public record CreateDriverCommand(CreateDriverRequest Request) : IRequest<DriverResponse>;

// Handler
public class CreateDriverHandler : IRequestHandler<CreateDriverCommand, DriverResponse>
{
    private readonly IRepository<Driver> _repository;

    public CreateDriverHandler(IRepository<Driver> repository)
    {
        _repository = repository;
    }

    public async Task<DriverResponse> Handle(CreateDriverCommand command, CancellationToken ct)
    {
        var driver = new Driver
        {
            Name = command.Request.Name,
            Email = command.Request.Email,
            PhoneNumber = command.Request.PhoneNumber,
            LicensePlate = command.Request.LicensePlate,
            MaxCargoWeightKg = command.Request.MaxCargoWeightKg,
            MaxCargoVolumeM3 = command.Request.MaxCargoVolumeM3,
            Status = DriverStatus.Offline
        };

        await _repository.AddAsync(driver, ct);

        return new DriverResponse(
            driver.Id, driver.Name, driver.Email, driver.PhoneNumber,
            driver.LicensePlate, driver.Status.ToString(),
            null, null,
            driver.MaxCargoWeightKg, driver.MaxCargoVolumeM3,
            driver.CreatedAt
        );
    }
}
```

Create `src/SmartLogistics.Application/Features/Drivers/Queries/GetAllDrivers.cs`:

```csharp
using MediatR;
using SmartLogistics.Application.DTOs;
using SmartLogistics.Domain.Entities;
using SmartLogistics.Domain.Interfaces;

namespace SmartLogistics.Application.Features.Drivers.Queries;

public record GetAllDriversQuery() : IRequest<IReadOnlyList<DriverResponse>>;

public class GetAllDriversHandler : IRequestHandler<GetAllDriversQuery, IReadOnlyList<DriverResponse>>
{
    private readonly IRepository<Driver> _repository;

    public GetAllDriversHandler(IRepository<Driver> repository)
    {
        _repository = repository;
    }

    public async Task<IReadOnlyList<DriverResponse>> Handle(GetAllDriversQuery query, CancellationToken ct)
    {
        var drivers = await _repository.GetAllAsync(ct);

        return drivers.Select(d => new DriverResponse(
            d.Id, d.Name, d.Email, d.PhoneNumber,
            d.LicensePlate, d.Status.ToString(),
            d.CurrentLocation?.Y, d.CurrentLocation?.X,
            d.MaxCargoWeightKg, d.MaxCargoVolumeM3,
            d.CreatedAt
        )).ToList();
    }
}
```

**Repeat this pattern for:** `GetDriverById`, `UpdateDriver`, `DeleteDriver`, `UpdateDriverLocation`, `UpdateDriverStatus`. Then do the same for Merchants and Zones.

### Step 3.5 — Add FluentValidation

Create `src/SmartLogistics.Application/Validators/CreateDriverValidator.cs`:

```csharp
using FluentValidation;
using SmartLogistics.Application.Features.Drivers.Commands;

namespace SmartLogistics.Application.Validators;

public class CreateDriverValidator : AbstractValidator<CreateDriverCommand>
{
    public CreateDriverValidator()
    {
        RuleFor(x => x.Request.Name).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Request.Email).NotEmpty().EmailAddress();
        RuleFor(x => x.Request.PhoneNumber).NotEmpty();
        RuleFor(x => x.Request.LicensePlate).NotEmpty().MaximumLength(20);
        RuleFor(x => x.Request.MaxCargoWeightKg).GreaterThan(0);
        RuleFor(x => x.Request.MaxCargoVolumeM3).GreaterThan(0);
    }
}
```

Create a MediatR validation pipeline behavior in `src/SmartLogistics.Application/Behaviors/ValidationBehavior.cs`:

```csharp
using FluentValidation;
using MediatR;

namespace SmartLogistics.Application.Behaviors;

public class ValidationBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    private readonly IEnumerable<IValidator<TRequest>> _validators;

    public ValidationBehavior(IEnumerable<IValidator<TRequest>> validators)
    {
        _validators = validators;
    }

    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken ct)
    {
        if (_validators.Any())
        {
            var context = new ValidationContext<TRequest>(request);
            var failures = (await Task.WhenAll(
                _validators.Select(v => v.ValidateAsync(context, ct))))
                .SelectMany(r => r.Errors)
                .Where(f => f != null)
                .ToList();

            if (failures.Any())
                throw new ValidationException(failures);
        }

        return await next();
    }
}
```

### Step 3.6 — Create the Driver controller

Create `src/SmartLogistics.API/Controllers/DriversController.cs`:

```csharp
using MediatR;
using Microsoft.AspNetCore.Mvc;
using SmartLogistics.Application.DTOs;
using SmartLogistics.Application.Features.Drivers.Commands;
using SmartLogistics.Application.Features.Drivers.Queries;

namespace SmartLogistics.API.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
public class DriversController : ControllerBase
{
    private readonly IMediator _mediator;

    public DriversController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll(CancellationToken ct)
    {
        var result = await _mediator.Send(new GetAllDriversQuery(), ct);
        return Ok(result);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id, CancellationToken ct)
    {
        var result = await _mediator.Send(new GetDriverByIdQuery(id), ct);
        return result is null ? NotFound() : Ok(result);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateDriverRequest request, CancellationToken ct)
    {
        var result = await _mediator.Send(new CreateDriverCommand(request), ct);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateDriverRequest request, CancellationToken ct)
    {
        await _mediator.Send(new UpdateDriverCommand(id, request), ct);
        return NoContent();
    }

    [HttpPatch("{id:guid}/location")]
    public async Task<IActionResult> UpdateLocation(Guid id, [FromBody] UpdateDriverLocationRequest request, CancellationToken ct)
    {
        await _mediator.Send(new UpdateDriverLocationCommand(id, request), ct);
        return NoContent();
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
    {
        await _mediator.Send(new DeleteDriverCommand(id), ct);
        return NoContent();
    }
}
```

### Step 3.7 — Register everything in DI (Program.cs)

Add to your `Program.cs`:

```csharp
using FluentValidation;
using MediatR;
using SmartLogistics.Application.Behaviors;
using SmartLogistics.Domain.Interfaces;
using SmartLogistics.Infrastructure.Repositories;

// MediatR
builder.Services.AddMediatR(cfg =>
    cfg.RegisterServicesFromAssembly(typeof(SmartLogistics.Application.Features.Drivers.Commands.CreateDriverCommand).Assembly));

// FluentValidation
builder.Services.AddValidatorsFromAssembly(typeof(SmartLogistics.Application.Validators.CreateDriverValidator).Assembly);
builder.Services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));

// Repositories
builder.Services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
```

### Step 3.8 — Add global exception handling middleware

Create `src/SmartLogistics.API/Middleware/ExceptionMiddleware.cs`:

```csharp
using System.Net;
using System.Text.Json;
using FluentValidation;

namespace SmartLogistics.API.Middleware;

public class ExceptionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionMiddleware> _logger;

    public ExceptionMiddleware(RequestDelegate next, ILogger<ExceptionMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (ValidationException ex)
        {
            context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
            context.Response.ContentType = "application/json";
            var errors = ex.Errors.Select(e => new { e.PropertyName, e.ErrorMessage });
            await context.Response.WriteAsync(JsonSerializer.Serialize(new { Errors = errors }));
        }
        catch (KeyNotFoundException ex)
        {
            context.Response.StatusCode = (int)HttpStatusCode.NotFound;
            context.Response.ContentType = "application/json";
            await context.Response.WriteAsync(JsonSerializer.Serialize(new { Error = ex.Message }));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unhandled exception");
            context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
            context.Response.ContentType = "application/json";
            await context.Response.WriteAsync(JsonSerializer.Serialize(new { Error = "An unexpected error occurred" }));
        }
    }
}
```

Register it in `Program.cs` before other middleware:

```csharp
app.UseMiddleware<ExceptionMiddleware>();
```

**✅ Checkpoint 3 complete when:** You can CRUD Drivers, Merchants, and Zones via Swagger. Validation errors return 400 with details. Invalid IDs return 404.

---

## ✅ Checkpoint 4 — JWT Authentication & Authorization

**Goal:** Secure your API with JWT tokens. Three roles: Admin, Driver, Merchant.

### Step 4.1 — Create a User entity

Create `src/SmartLogistics.Domain/Entities/User.cs`:

```csharp
namespace SmartLogistics.Domain.Entities;

public class User : BaseEntity
{
    public string Email { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public UserRole Role { get; set; }
    public Guid? DriverId { get; set; }   // If role is Driver
    public Guid? MerchantId { get; set; } // If role is Merchant
}

public enum UserRole
{
    Admin,
    Driver,
    Merchant
}
```

Add `DbSet<User> Users` to `AppDbContext` and configure it. Run a new migration.

### Step 4.2 — Create JWT service

Create `src/SmartLogistics.Infrastructure/Auth/JwtService.cs`:

```csharp
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using SmartLogistics.Domain.Entities;

namespace SmartLogistics.Infrastructure.Auth;

public interface IJwtService
{
    string GenerateToken(User user);
}

public class JwtService : IJwtService
{
    private readonly IConfiguration _config;

    public JwtService(IConfiguration config) => _config = config;

    public string GenerateToken(User user)
    {
        var key = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(_config["JwtSettings:Secret"]!));

        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new(ClaimTypes.Email, user.Email),
            new(ClaimTypes.Role, user.Role.ToString())
        };

        if (user.DriverId.HasValue)
            claims.Add(new Claim("DriverId", user.DriverId.Value.ToString()));
        if (user.MerchantId.HasValue)
            claims.Add(new Claim("MerchantId", user.MerchantId.Value.ToString()));

        var token = new JwtSecurityToken(
            issuer: _config["JwtSettings:Issuer"],
            audience: _config["JwtSettings:Audience"],
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(
                double.Parse(_config["JwtSettings:ExpirationInMinutes"]!)),
            signingCredentials: new SigningCredentials(key, SecurityAlgorithms.HmacSha256)
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
```

### Step 4.3 — Create Auth controller

Create `src/SmartLogistics.API/Controllers/AuthController.cs`:

```csharp
using MediatR;
using Microsoft.AspNetCore.Mvc;
using SmartLogistics.Application.Features.Auth.Commands;

namespace SmartLogistics.API.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IMediator _mediator;

    public AuthController(IMediator mediator) => _mediator = mediator;

    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterCommand command, CancellationToken ct)
    {
        var result = await _mediator.Send(command, ct);
        return Ok(result);
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginCommand command, CancellationToken ct)
    {
        var result = await _mediator.Send(command, ct);
        return Ok(result);
    }
}
```

You need to create the corresponding `RegisterCommand`/`LoginCommand` MediatR handlers that hash the password (use `BCrypt.Net-Next` NuGet package), store the user, and return a JWT.

### Step 4.4 — Configure JWT in Program.cs

```csharp
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["JwtSettings:Issuer"],
            ValidAudience = builder.Configuration["JwtSettings:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(builder.Configuration["JwtSettings:Secret"]!))
        };
    });

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("AdminOnly", p => p.RequireRole("Admin"));
    options.AddPolicy("DriverOnly", p => p.RequireRole("Driver"));
    options.AddPolicy("MerchantOnly", p => p.RequireRole("Merchant"));
});
```

Add middleware (order matters):

```csharp
app.UseAuthentication();  // Before UseAuthorization
app.UseAuthorization();
```

### Step 4.5 — Protect endpoints

Add `[Authorize]` attribute to controllers and use policies:

```csharp
[Authorize]  // Any authenticated user
[HttpGet]
public async Task<IActionResult> GetAll() { ... }

[Authorize(Policy = "AdminOnly")]  // Only admins
[HttpDelete("{id:guid}")]
public async Task<IActionResult> Delete(Guid id) { ... }
```

**✅ Checkpoint 4 complete when:** You can register, login, receive a JWT, and use it in Swagger's "Authorize" button. Protected endpoints return 401 without a token and 403 with the wrong role.

---

## ✅ Checkpoint 5 — Order Lifecycle & State Machine

**Goal:** Implement the full order lifecycle with proper state transitions and domain events.

### Step 5.1 — Create domain events

Create `src/SmartLogistics.Domain/Events/OrderCreatedEvent.cs`:

```csharp
using MediatR;

namespace SmartLogistics.Domain.Events;

public record OrderCreatedEvent(Guid OrderId, Guid MerchantId, double Latitude, double Longitude) : INotification;
public record OrderAssignedEvent(Guid OrderId, Guid DriverId) : INotification;
public record DeliveryCompletedEvent(Guid DeliveryId, Guid OrderId, Guid DriverId) : INotification;
public record DeliveryFailedEvent(Guid DeliveryId, Guid OrderId, Guid DriverId, string Reason) : INotification;
```

### Step 5.2 — Add state transition logic to the Order entity

Add this method to `Order.cs`:

```csharp
public void TransitionTo(OrderStatus newStatus)
{
    var allowed = Status switch
    {
        OrderStatus.Created => new[] { OrderStatus.Assigned, OrderStatus.Cancelled },
        OrderStatus.Assigned => new[] { OrderStatus.PickedUp, OrderStatus.Cancelled },
        OrderStatus.PickedUp => new[] { OrderStatus.InTransit },
        OrderStatus.InTransit => new[] { OrderStatus.Delivered, OrderStatus.Failed },
        _ => Array.Empty<OrderStatus>()
    };

    if (!allowed.Contains(newStatus))
        throw new InvalidOperationException(
            $"Cannot transition order from {Status} to {newStatus}");

    Status = newStatus;
    UpdatedAt = DateTime.UtcNow;
}
```

### Step 5.3 — Create Order endpoints

Your `OrdersController` should have these endpoints:

```
POST   /api/v1/orders             → Create order (Merchant role)
GET    /api/v1/orders/{id}        → Get order details
GET    /api/v1/orders             → List orders (with filters: status, merchant, date range)
PATCH  /api/v1/orders/{id}/assign → Assign a driver
PATCH  /api/v1/orders/{id}/pickup → Mark as picked up (Driver role)
PATCH  /api/v1/orders/{id}/deliver → Mark as delivered (Driver role)
PATCH  /api/v1/orders/{id}/fail   → Mark as failed (Driver role)
PATCH  /api/v1/orders/{id}/cancel → Cancel order
```

Each state transition command should publish the appropriate domain event via MediatR's `IPublisher`.

### Step 5.4 — Handle domain events

Create `src/SmartLogistics.Application/Features/Orders/EventHandlers/OrderCreatedHandler.cs`:

```csharp
using MediatR;
using SmartLogistics.Domain.Events;

namespace SmartLogistics.Application.Features.Orders.EventHandlers;

public class OrderCreatedHandler : INotificationHandler<OrderCreatedEvent>
{
    private readonly ILogger<OrderCreatedHandler> _logger;

    public OrderCreatedHandler(ILogger<OrderCreatedHandler> logger) => _logger = logger;

    public Task Handle(OrderCreatedEvent notification, CancellationToken ct)
    {
        _logger.LogInformation(
            "Order {OrderId} created by merchant {MerchantId}. Ready for dispatch.",
            notification.OrderId, notification.MerchantId);

        // Later: this will trigger the dispatch engine (Checkpoint 7)
        return Task.CompletedTask;
    }
}
```

**✅ Checkpoint 5 complete when:** You can create an order and walk it through its full lifecycle. Invalid transitions return errors. Domain events fire and log correctly.

---

## ✅ Checkpoint 6 — Graph Algorithms (Routing & Zones)

**Goal:** Implement Dijkstra's shortest path and a greedy TSP heuristic for multi-stop route optimization.

### Step 6.1 — Build the graph from zone connections

Create `src/SmartLogistics.Application/Services/RoutingService.cs`:

```csharp
using SmartLogistics.Domain.Entities;
using SmartLogistics.Domain.Interfaces;

namespace SmartLogistics.Application.Services;

public class RoutingService
{
    private readonly IRepository<Zone> _zoneRepo;
    private readonly IRepository<ZoneConnection> _connectionRepo;

    public RoutingService(IRepository<Zone> zoneRepo, IRepository<ZoneConnection> connectionRepo)
    {
        _zoneRepo = zoneRepo;
        _connectionRepo = connectionRepo;
    }

    /// <summary>
    /// Dijkstra's algorithm — find shortest path between two zones.
    /// Returns the ordered list of zone IDs and total cost.
    /// </summary>
    public async Task<(List<Guid> Path, double TotalCost)> FindShortestPath(
        Guid fromZoneId, Guid toZoneId, CancellationToken ct)
    {
        var connections = await _connectionRepo.GetAllAsync(ct);

        // Build adjacency list
        var graph = new Dictionary<Guid, List<(Guid To, double Cost)>>();
        foreach (var conn in connections)
        {
            if (!graph.ContainsKey(conn.FromZoneId))
                graph[conn.FromZoneId] = new List<(Guid, double)>();

            double cost = conn.DistanceKm * conn.TrafficMultiplier;
            graph[conn.FromZoneId].Add((conn.ToZoneId, cost));
        }

        // Dijkstra
        var distances = new Dictionary<Guid, double>();
        var previous = new Dictionary<Guid, Guid?>();
        var visited = new HashSet<Guid>();
        var pq = new PriorityQueue<Guid, double>();

        foreach (var zone in graph.Keys)
            distances[zone] = double.MaxValue;

        distances[fromZoneId] = 0;
        pq.Enqueue(fromZoneId, 0);

        while (pq.Count > 0)
        {
            var current = pq.Dequeue();
            if (visited.Contains(current)) continue;
            visited.Add(current);

            if (current == toZoneId) break;

            if (!graph.ContainsKey(current)) continue;

            foreach (var (neighbor, cost) in graph[current])
            {
                var newDist = distances[current] + cost;
                if (newDist < distances.GetValueOrDefault(neighbor, double.MaxValue))
                {
                    distances[neighbor] = newDist;
                    previous[neighbor] = current;
                    pq.Enqueue(neighbor, newDist);
                }
            }
        }

        // Reconstruct path
        var path = new List<Guid>();
        var node = toZoneId;
        while (previous.ContainsKey(node) && previous[node] != null)
        {
            path.Add(node);
            node = previous[node]!.Value;
        }
        path.Add(fromZoneId);
        path.Reverse();

        return (path, distances.GetValueOrDefault(toZoneId, -1));
    }

    /// <summary>
    /// Greedy nearest-neighbor TSP heuristic for multi-stop delivery optimization.
    /// Returns stops in optimized order.
    /// </summary>
    public async Task<List<Guid>> OptimizeMultiStopRoute(
        Guid startZoneId, List<Guid> stopZoneIds, CancellationToken ct)
    {
        var remaining = new HashSet<Guid>(stopZoneIds);
        var route = new List<Guid>();
        var current = startZoneId;

        while (remaining.Count > 0)
        {
            Guid nearest = default;
            double nearestCost = double.MaxValue;

            foreach (var stop in remaining)
            {
                var (_, cost) = await FindShortestPath(current, stop, ct);
                if (cost >= 0 && cost < nearestCost)
                {
                    nearestCost = cost;
                    nearest = stop;
                }
            }

            if (nearest == default) break;

            route.Add(nearest);
            remaining.Remove(nearest);
            current = nearest;
        }

        return route;
    }
}
```

### Step 6.2 — Create routing endpoints

Create `src/SmartLogistics.API/Controllers/RoutingController.cs`:

```csharp
[ApiController]
[Route("api/v1/[controller]")]
[Authorize]
public class RoutingController : ControllerBase
{
    private readonly RoutingService _routingService;

    public RoutingController(RoutingService routingService)
        => _routingService = routingService;

    [HttpGet("shortest-path")]
    public async Task<IActionResult> GetShortestPath(
        [FromQuery] Guid fromZoneId, [FromQuery] Guid toZoneId, CancellationToken ct)
    {
        var (path, cost) = await _routingService.FindShortestPath(fromZoneId, toZoneId, ct);
        return Ok(new { Path = path, TotalCost = cost });
    }

    [HttpPost("optimize-route")]
    public async Task<IActionResult> OptimizeRoute(
        [FromBody] OptimizeRouteRequest request, CancellationToken ct)
    {
        var optimized = await _routingService.OptimizeMultiStopRoute(
            request.StartZoneId, request.StopZoneIds, ct);
        return Ok(new { OptimizedStops = optimized });
    }
}

public record OptimizeRouteRequest(Guid StartZoneId, List<Guid> StopZoneIds);
```

### Step 6.3 — Seed test zone data

Create a seeding endpoint (Admin only) or a migration seed that creates ~10-15 zones with connections forming a realistic graph. Make sure some connections have higher traffic multipliers to make Dijkstra's choices interesting.

**✅ Checkpoint 6 complete when:** You can query the shortest path between two zones and get an optimized multi-stop route. Write unit tests for Dijkstra with known graphs to verify correctness.

---

## ✅ Checkpoint 7 — Dispatch Engine (BackgroundService + Priority Queue)

**Goal:** A continuously running background service that matches pending orders to available drivers using a priority queue.

### Step 7.1 — Create the dispatch engine

Create `src/SmartLogistics.Infrastructure/Services/DispatchEngine.cs`:

```csharp
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using SmartLogistics.Domain.Entities;
using SmartLogistics.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using NetTopologySuite.Geometries;

namespace SmartLogistics.Infrastructure.Services;

public class DispatchEngine : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<DispatchEngine> _logger;

    public DispatchEngine(IServiceProvider serviceProvider, ILogger<DispatchEngine> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Dispatch Engine started");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                using var scope = _serviceProvider.CreateScope();
                var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

                await DispatchPendingOrders(db, stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Dispatch engine error");
            }

            await Task.Delay(TimeSpan.FromSeconds(5), stoppingToken); // Poll every 5s
        }
    }

    private async Task DispatchPendingOrders(AppDbContext db, CancellationToken ct)
    {
        // Get pending orders, ordered by priority (Urgent first) then by creation time
        var pendingOrders = await db.Orders
            .Where(o => o.Status == OrderStatus.Created)
            .OrderByDescending(o => o.Priority)
            .ThenBy(o => o.CreatedAt)
            .Take(10) // Process in batches
            .ToListAsync(ct);

        if (!pendingOrders.Any()) return;

        // Get available drivers with their locations
        var availableDrivers = await db.Drivers
            .Where(d => d.Status == DriverStatus.Available && d.CurrentLocation != null)
            .ToListAsync(ct);

        if (!availableDrivers.Any())
        {
            _logger.LogWarning("No available drivers for {Count} pending orders", pendingOrders.Count);
            return;
        }

        foreach (var order in pendingOrders)
        {
            // Find nearest available driver to the pickup location
            var nearestDriver = availableDrivers
                .OrderBy(d => d.CurrentLocation!.Distance(order.PickupLocation))
                .FirstOrDefault(d => d.MaxCargoWeightKg >= order.WeightKg
                                  && d.MaxCargoVolumeM3 >= order.VolumeM3);

            if (nearestDriver == null)
            {
                _logger.LogWarning("No suitable driver found for order {OrderId}", order.Id);
                continue;
            }

            // Assign
            order.TransitionTo(OrderStatus.Assigned);
            nearestDriver.Status = DriverStatus.OnDelivery;

            var delivery = new Delivery
            {
                OrderId = order.Id,
                DriverId = nearestDriver.Id,
                Status = DeliveryStatus.DriverEnRoute
            };

            db.Deliveries.Add(delivery);
            availableDrivers.Remove(nearestDriver); // Don't double-assign

            _logger.LogInformation(
                "Assigned order {OrderId} (priority: {Priority}) to driver {DriverId}",
                order.Id, order.Priority, nearestDriver.Id);
        }

        await db.SaveChangesAsync(ct);
    }
}
```

### Step 7.2 — Register the background service

In `Program.cs`:

```csharp
builder.Services.AddHostedService<DispatchEngine>();
```

**✅ Checkpoint 7 complete when:** Create a driver (set them as Available with a location), create an order, and within 5 seconds the dispatch engine assigns the driver. Check the logs.

---

## ✅ Checkpoint 8 — Kafka Integration

**Goal:** Publish order events to Kafka topics. Create a consumer background service that processes them.

### Step 8.1 — Create a Kafka producer service

Create `src/SmartLogistics.Infrastructure/Messaging/KafkaProducer.cs`:

```csharp
using System.Text.Json;
using Confluent.Kafka;
using Microsoft.Extensions.Configuration;

namespace SmartLogistics.Infrastructure.Messaging;

public interface IEventPublisher
{
    Task PublishAsync<T>(string topic, T message, CancellationToken ct = default);
}

public class KafkaProducer : IEventPublisher, IDisposable
{
    private readonly IProducer<string, string> _producer;

    public KafkaProducer(IConfiguration config)
    {
        var producerConfig = new ProducerConfig
        {
            BootstrapServers = config["Kafka:BootstrapServers"]
        };
        _producer = new ProducerBuilder<string, string>(producerConfig).Build();
    }

    public async Task PublishAsync<T>(string topic, T message, CancellationToken ct = default)
    {
        var json = JsonSerializer.Serialize(message);
        await _producer.ProduceAsync(topic, new Message<string, string>
        {
            Key = Guid.NewGuid().ToString(),
            Value = json
        }, ct);
    }

    public void Dispose() => _producer.Dispose();
}
```

### Step 8.2 — Create a Kafka consumer background service

Create `src/SmartLogistics.Infrastructure/Messaging/OrderEventsConsumer.cs`:

```csharp
using System.Text.Json;
using Confluent.Kafka;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace SmartLogistics.Infrastructure.Messaging;

public class OrderEventsConsumer : BackgroundService
{
    private readonly ILogger<OrderEventsConsumer> _logger;
    private readonly IConsumer<string, string> _consumer;
    private readonly IServiceProvider _serviceProvider;

    public OrderEventsConsumer(
        IConfiguration config,
        ILogger<OrderEventsConsumer> logger,
        IServiceProvider serviceProvider)
    {
        _logger = logger;
        _serviceProvider = serviceProvider;

        var consumerConfig = new ConsumerConfig
        {
            BootstrapServers = config["Kafka:BootstrapServers"],
            GroupId = config["Kafka:GroupId"],
            AutoOffsetReset = AutoOffsetReset.Earliest
        };
        _consumer = new ConsumerBuilder<string, string>(consumerConfig).Build();
    }

    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        return Task.Run(() =>
        {
            _consumer.Subscribe(new[] { "order-events", "delivery-events" });

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    var result = _consumer.Consume(stoppingToken);
                    _logger.LogInformation(
                        "Consumed message from {Topic}: {Value}",
                        result.Topic, result.Message.Value);

                    // Process based on topic
                    // Use _serviceProvider.CreateScope() to resolve services
                }
                catch (ConsumeException ex)
                {
                    _logger.LogError(ex, "Kafka consume error");
                }
            }
        }, stoppingToken);
    }

    public override void Dispose()
    {
        _consumer.Close();
        _consumer.Dispose();
        base.Dispose();
    }
}
```

### Step 8.3 — Publish events from domain event handlers

Update your `OrderCreatedHandler` to publish to Kafka:

```csharp
public class OrderCreatedHandler : INotificationHandler<OrderCreatedEvent>
{
    private readonly IEventPublisher _publisher;
    private readonly ILogger<OrderCreatedHandler> _logger;

    public OrderCreatedHandler(IEventPublisher publisher, ILogger<OrderCreatedHandler> logger)
    {
        _publisher = publisher;
        _logger = logger;
    }

    public async Task Handle(OrderCreatedEvent notification, CancellationToken ct)
    {
        await _publisher.PublishAsync("order-events", notification, ct);
        _logger.LogInformation("Published OrderCreated event for {OrderId}", notification.OrderId);
    }
}
```

### Step 8.4 — Register in DI

```csharp
builder.Services.AddSingleton<IEventPublisher, KafkaProducer>();
builder.Services.AddHostedService<OrderEventsConsumer>();
```

**✅ Checkpoint 8 complete when:** Creating an order publishes an event to Kafka. The consumer picks it up and logs it. Use `docker exec` into the Kafka container to verify messages on the topic.

---

## ✅ Checkpoint 9 — Redis Caching & Distributed Locking

**Goal:** Cache driver locations in Redis for fast geospatial queries. Add distributed locks to prevent double-dispatch.

### Step 9.1 — Create a Redis caching service

Create `src/SmartLogistics.Infrastructure/Caching/RedisCacheService.cs`:

```csharp
using StackExchange.Redis;
using System.Text.Json;

namespace SmartLogistics.Infrastructure.Caching;

public interface ICacheService
{
    Task<T?> GetAsync<T>(string key, CancellationToken ct = default);
    Task SetAsync<T>(string key, T value, TimeSpan? expiration = null, CancellationToken ct = default);
    Task RemoveAsync(string key, CancellationToken ct = default);
    Task<bool> AcquireLockAsync(string key, TimeSpan expiration);
    Task ReleaseLockAsync(string key);
}

public class RedisCacheService : ICacheService
{
    private readonly IDatabase _db;

    public RedisCacheService(IConnectionMultiplexer redis)
    {
        _db = redis.GetDatabase();
    }

    public async Task<T?> GetAsync<T>(string key, CancellationToken ct = default)
    {
        var value = await _db.StringGetAsync(key);
        return value.IsNullOrEmpty ? default : JsonSerializer.Deserialize<T>(value!);
    }

    public async Task SetAsync<T>(string key, T value, TimeSpan? expiration = null, CancellationToken ct = default)
    {
        var json = JsonSerializer.Serialize(value);
        await _db.StringSetAsync(key, json, expiration);
    }

    public async Task RemoveAsync(string key, CancellationToken ct = default)
    {
        await _db.KeyDeleteAsync(key);
    }

    public async Task<bool> AcquireLockAsync(string key, TimeSpan expiration)
    {
        return await _db.StringSetAsync($"lock:{key}", "locked", expiration, When.NotExists);
    }

    public async Task ReleaseLockAsync(string key)
    {
        await _db.KeyDeleteAsync($"lock:{key}");
    }
}
```

### Step 9.2 — Use Redis geospatial commands for driver locations

Create `src/SmartLogistics.Infrastructure/Caching/DriverLocationCache.cs`:

```csharp
using StackExchange.Redis;

namespace SmartLogistics.Infrastructure.Caching;

public interface IDriverLocationCache
{
    Task UpdateLocationAsync(Guid driverId, double latitude, double longitude);
    Task<List<(Guid DriverId, double Distance)>> FindNearestDriversAsync(
        double latitude, double longitude, double radiusKm, int count = 5);
    Task RemoveDriverAsync(Guid driverId);
}

public class DriverLocationCache : IDriverLocationCache
{
    private readonly IDatabase _db;
    private const string GeoKey = "driver:locations";

    public DriverLocationCache(IConnectionMultiplexer redis) => _db = redis.GetDatabase();

    public async Task UpdateLocationAsync(Guid driverId, double latitude, double longitude)
    {
        await _db.GeoAddAsync(GeoKey, longitude, latitude, driverId.ToString());
    }

    public async Task<List<(Guid DriverId, double Distance)>> FindNearestDriversAsync(
        double latitude, double longitude, double radiusKm, int count = 5)
    {
        var results = await _db.GeoSearchAsync(GeoKey,
            longitude, latitude,
            new GeoSearchCircle(radiusKm, GeoUnit.Kilometers),
            count: count,
            order: Order.Ascending,
            options: GeoRadiusOptions.WithDistance);

        return results.Select(r => (
            Guid.Parse(r.Member.ToString()),
            r.Distance ?? 0
        )).ToList();
    }

    public async Task RemoveDriverAsync(Guid driverId)
    {
        await _db.GeoRemoveAsync(GeoKey, driverId.ToString());
    }
}
```

### Step 9.3 — Register Redis in DI

```csharp
using StackExchange.Redis;

var redisConnection = ConnectionMultiplexer.Connect(
    builder.Configuration.GetConnectionString("Redis")!);

builder.Services.AddSingleton<IConnectionMultiplexer>(redisConnection);
builder.Services.AddSingleton<ICacheService, RedisCacheService>();
builder.Services.AddSingleton<IDriverLocationCache, DriverLocationCache>();
```

### Step 9.4 — Update the dispatch engine to use Redis geospatial + distributed locks

Replace the simple LINQ distance query in the dispatch engine with `IDriverLocationCache.FindNearestDriversAsync()`. Wrap the assignment in a distributed lock to prevent double-dispatch:

```csharp
var lockAcquired = await _cache.AcquireLockAsync($"dispatch:order:{order.Id}", TimeSpan.FromSeconds(30));
if (!lockAcquired) continue;

try
{
    // ... assign driver ...
}
finally
{
    await _cache.ReleaseLockAsync($"dispatch:order:{order.Id}");
}
```

**✅ Checkpoint 9 complete when:** Driver locations are stored/queried from Redis. The dispatch engine uses geospatial queries and distributed locks.

---

## ✅ Checkpoint 10 — Resilience with Polly & External API Calls

**Goal:** Add an external API call (geocoding/maps) with Polly retry and circuit breaker policies.

### Step 10.1 — Create a typed HTTP client

Create `src/SmartLogistics.Infrastructure/ExternalServices/GeocodingService.cs`:

```csharp
using System.Text.Json;

namespace SmartLogistics.Infrastructure.ExternalServices;

public interface IGeocodingService
{
    Task<(double Latitude, double Longitude)?> GeocodeAddressAsync(string address, CancellationToken ct);
}

public class GeocodingService : IGeocodingService
{
    private readonly HttpClient _httpClient;

    public GeocodingService(HttpClient httpClient) => _httpClient = httpClient;

    public async Task<(double Latitude, double Longitude)?> GeocodeAddressAsync(
        string address, CancellationToken ct)
    {
        // Using a free geocoding API (replace with your preferred provider)
        var response = await _httpClient.GetAsync(
            $"https://nominatim.openstreetmap.org/search?q={Uri.EscapeDataString(address)}&format=json&limit=1",
            ct);

        response.EnsureSuccessStatusCode();

        var json = await response.Content.ReadAsStringAsync(ct);
        var results = JsonSerializer.Deserialize<JsonElement[]>(json);

        if (results == null || results.Length == 0) return null;

        var lat = double.Parse(results[0].GetProperty("lat").GetString()!);
        var lon = double.Parse(results[0].GetProperty("lon").GetString()!);
        return (lat, lon);
    }
}
```

### Step 10.2 — Register with Polly policies

```csharp
using Polly;
using Polly.Extensions.Http;

builder.Services.AddHttpClient<IGeocodingService, GeocodingService>(client =>
{
    client.DefaultRequestHeaders.Add("User-Agent", "SmartLogisticsAPI/1.0");
    client.Timeout = TimeSpan.FromSeconds(10);
})
.AddPolicyHandler(HttpPolicyExtensions
    .HandleTransientHttpError()
    .WaitAndRetryAsync(3, retryAttempt =>
        TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)),  // Exponential backoff
        onRetry: (exception, timespan, retryCount, context) =>
        {
            // Log the retry
        }))
.AddPolicyHandler(HttpPolicyExtensions
    .HandleTransientHttpError()
    .CircuitBreakerAsync(5, TimeSpan.FromSeconds(30)));  // Open circuit after 5 failures
```

### Step 10.3 — Add health checks

```csharp
builder.Services.AddHealthChecks()
    .AddNpgSql(builder.Configuration.GetConnectionString("DefaultConnection")!)
    .AddRedis(builder.Configuration.GetConnectionString("Redis")!)
    .AddKafka(new Confluent.Kafka.ProducerConfig
    {
        BootstrapServers = builder.Configuration["Kafka:BootstrapServers"]
    });

// In the pipeline:
app.MapHealthChecks("/health");
```

You'll need:
```bash
dotnet add src/SmartLogistics.API package AspNetCore.HealthChecks.NpgSql
dotnet add src/SmartLogistics.API package AspNetCore.HealthChecks.Redis
dotnet add src/SmartLogistics.API package AspNetCore.HealthChecks.Kafka
```

**✅ Checkpoint 10 complete when:** The geocoding call retries on failure and trips a circuit breaker. `/health` reports the status of Postgres, Redis, and Kafka.

---

## ✅ Checkpoint 11 — Advanced DSA Features

**Goal:** Implement knapsack for load optimization, sliding window for driver stats, and rate limiting.

### Step 11.1 — Knapsack: Batch delivery optimization

Create `src/SmartLogistics.Application/Services/LoadOptimizer.cs`:

```csharp
namespace SmartLogistics.Application.Services;

public class LoadOptimizer
{
    /// <summary>
    /// 0/1 Knapsack — select the best combination of orders that fit
    /// in the vehicle's capacity while maximizing priority value.
    /// </summary>
    public List<int> OptimizeLoad(
        List<(double Weight, double Volume, int PriorityValue)> orders,
        double maxWeight,
        double maxVolume)
    {
        int n = orders.Count;

        // Discretize weights and volumes for DP (multiply by 10 for 1 decimal precision)
        int W = (int)(maxWeight * 10);
        int V = (int)(maxVolume * 10);

        // Since 3D DP might be too large, use a greedy approach sorted by value density
        var indexed = orders
            .Select((o, i) => new
            {
                Index = i,
                o.Weight,
                o.Volume,
                o.PriorityValue,
                Density = o.PriorityValue / (o.Weight + o.Volume + 0.01)
            })
            .OrderByDescending(x => x.Density)
            .ToList();

        var selected = new List<int>();
        double currentWeight = 0, currentVolume = 0;

        foreach (var item in indexed)
        {
            if (currentWeight + item.Weight <= maxWeight &&
                currentVolume + item.Volume <= maxVolume)
            {
                selected.Add(item.Index);
                currentWeight += item.Weight;
                currentVolume += item.Volume;
            }
        }

        return selected;
    }
}
```

### Step 11.2 — Sliding window: Driver performance stats

Create `src/SmartLogistics.Application/Services/DriverStatsService.cs`:

```csharp
namespace SmartLogistics.Application.Services;

public class DriverStatsService
{
    /// <summary>
    /// Calculate delivery success rate over a sliding window of N deliveries.
    /// </summary>
    public double CalculateSuccessRate(List<bool> deliveryResults, int windowSize)
    {
        if (deliveryResults.Count == 0) return 0;

        int window = Math.Min(windowSize, deliveryResults.Count);
        int successes = 0;

        // Initialize first window
        for (int i = deliveryResults.Count - window; i < deliveryResults.Count; i++)
        {
            if (deliveryResults[i]) successes++;
        }

        return (double)successes / window * 100;
    }

    /// <summary>
    /// Find the longest delivery streak (consecutive successes).
    /// </summary>
    public int LongestSuccessStreak(List<bool> deliveryResults)
    {
        int maxStreak = 0, current = 0;
        foreach (var success in deliveryResults)
        {
            current = success ? current + 1 : 0;
            maxStreak = Math.Max(maxStreak, current);
        }
        return maxStreak;
    }
}
```

### Step 11.3 — Rate limiting middleware

Add rate limiting per API key using `AspNetCoreRateLimit` (already installed):

```csharp
// In Program.cs
builder.Services.AddMemoryCache();
builder.Services.Configure<ClientRateLimitOptions>(options =>
{
    options.GeneralRules = new List<RateLimitRule>
    {
        new RateLimitRule
        {
            Endpoint = "*",
            Period = "1m",
            Limit = 60
        },
        new RateLimitRule
        {
            Endpoint = "post:/api/v1/orders",
            Period = "1m",
            Limit = 20
        }
    };
});
builder.Services.AddInMemoryRateLimiting();
builder.Services.AddSingleton<IRateLimitConfiguration, RateLimitConfiguration>();

// In the pipeline (before routing):
app.UseClientRateLimiting();
```

### Step 11.4 — Create analytics endpoints

```
GET /api/v1/analytics/drivers/{id}/stats     → Success rate, streak, avg delivery time
GET /api/v1/analytics/orders/summary         → Orders by status, avg time per status
POST /api/v1/dispatch/optimize-load          → Knapsack: best orders for a given vehicle
```

**✅ Checkpoint 11 complete when:** The knapsack optimizer selects the best orders for a vehicle. Driver stats calculate over a sliding window. Rate limiting blocks excessive requests.

---

## ✅ Checkpoint 12 — Testing

**Goal:** Comprehensive unit and integration tests.

### Step 12.1 — Unit tests for algorithms

Create `tests/SmartLogistics.UnitTests/Services/RoutingServiceTests.cs`:

```csharp
using FluentAssertions;

namespace SmartLogistics.UnitTests.Services;

public class LoadOptimizerTests
{
    [Fact]
    public void OptimizeLoad_ShouldSelectHighestPriorityItems()
    {
        var optimizer = new LoadOptimizer();
        var orders = new List<(double Weight, double Volume, int PriorityValue)>
        {
            (5, 1, 10),   // Heavy, high value
            (2, 0.5, 8),  // Light, good value
            (3, 0.5, 7),  // Medium
            (8, 2, 15),   // Too heavy alone with others
        };

        var selected = optimizer.OptimizeLoad(orders, maxWeight: 10, maxVolume: 3);

        selected.Should().NotBeEmpty();
        // Verify total weight and volume are within limits
        var totalWeight = selected.Sum(i => orders[i].Weight);
        var totalVolume = selected.Sum(i => orders[i].Volume);
        totalWeight.Should().BeLessThanOrEqualTo(10);
        totalVolume.Should().BeLessThanOrEqualTo(3);
    }

    [Fact]
    public void SlidingWindow_ShouldCalculateCorrectSuccessRate()
    {
        var stats = new DriverStatsService();
        var results = new List<bool> { true, true, false, true, true, true, false, true };

        var rate = stats.CalculateSuccessRate(results, windowSize: 5);

        rate.Should().Be(60); // 3 out of last 5
    }
}
```

### Step 12.2 — Integration tests with WebApplicationFactory

Create `tests/SmartLogistics.IntegrationTests/ApiTests/DriversApiTests.cs`:

```csharp
using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;

namespace SmartLogistics.IntegrationTests.ApiTests;

public class DriversApiTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;

    public DriversApiTests(WebApplicationFactory<Program> factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task CreateDriver_ShouldReturn201()
    {
        var request = new
        {
            Name = "John Doe",
            Email = "john@test.com",
            PhoneNumber = "+1234567890",
            LicensePlate = "ABC-1234",
            MaxCargoWeightKg = 100.0,
            MaxCargoVolumeM3 = 5.0
        };

        var response = await _client.PostAsJsonAsync("/api/v1/drivers", request);

        response.StatusCode.Should().Be(HttpStatusCode.Created);
    }

    [Fact]
    public async Task CreateDriver_WithInvalidEmail_ShouldReturn400()
    {
        var request = new
        {
            Name = "John Doe",
            Email = "not-an-email",
            PhoneNumber = "+1234567890",
            LicensePlate = "ABC-1234",
            MaxCargoWeightKg = 100.0,
            MaxCargoVolumeM3 = 5.0
        };

        var response = await _client.PostAsJsonAsync("/api/v1/drivers", request);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }
}
```

For proper integration tests, you'll want a custom `WebApplicationFactory` that uses Testcontainers to spin up a real PostgreSQL container. Research `Testcontainers.PostgreSql` for the setup pattern.

### Step 12.3 — Run tests

```bash
dotnet test --verbosity normal
```

**✅ Checkpoint 12 complete when:** Unit tests pass for all algorithms. Integration tests verify API endpoints return correct status codes and responses.

---

## ✅ Checkpoint 13 — Dockerize & CI

**Goal:** Containerize the API and set up a GitHub Actions CI pipeline.

### Step 13.1 — Create a Dockerfile

Create `src/SmartLogistics.API/Dockerfile`:

```dockerfile
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["src/SmartLogistics.API/SmartLogistics.API.csproj", "src/SmartLogistics.API/"]
COPY ["src/SmartLogistics.Application/SmartLogistics.Application.csproj", "src/SmartLogistics.Application/"]
COPY ["src/SmartLogistics.Domain/SmartLogistics.Domain.csproj", "src/SmartLogistics.Domain/"]
COPY ["src/SmartLogistics.Infrastructure/SmartLogistics.Infrastructure.csproj", "src/SmartLogistics.Infrastructure/"]
RUN dotnet restore "src/SmartLogistics.API/SmartLogistics.API.csproj"
COPY . .
WORKDIR "/src/src/SmartLogistics.API"
RUN dotnet build -c Release -o /app/build

FROM build AS publish
RUN dotnet publish -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "SmartLogistics.API.dll"]
```

### Step 13.2 — Add the API to docker-compose

Add to your `docker-compose.yml`:

```yaml
  api:
    build:
      context: .
      dockerfile: src/SmartLogistics.API/Dockerfile
    container_name: smartlogistics-api
    ports:
      - "8080:8080"
    environment:
      - ASPNETCORE_ENVIRONMENT=Development
      - ConnectionStrings__DefaultConnection=Host=postgres;Port=5432;Database=smartlogistics;Username=smartlogistics;Password=devpassword123
      - ConnectionStrings__Redis=redis:6379
      - Kafka__BootstrapServers=kafka:29092
    depends_on:
      - postgres
      - redis
      - kafka
```

### Step 13.3 — GitHub Actions CI

Create `.github/workflows/ci.yml`:

```yaml
name: CI

on:
  push:
    branches: [main]
  pull_request:
    branches: [main]

jobs:
  build-and-test:
    runs-on: ubuntu-latest

    steps:
      - uses: actions/checkout@v4

      - name: Setup .NET
        uses: actions/setup-dotnet@v4
        with:
          dotnet-version: '8.0.x'

      - name: Restore dependencies
        run: dotnet restore

      - name: Build
        run: dotnet build --no-restore -c Release

      - name: Run unit tests
        run: dotnet test tests/SmartLogistics.UnitTests --no-build -c Release --verbosity normal

      - name: Run integration tests
        run: dotnet test tests/SmartLogistics.IntegrationTests --no-build -c Release --verbosity normal
```

### Step 13.4 — Test the full stack

```bash
docker-compose up --build
# API should be running at http://localhost:8080/swagger
```

**✅ Checkpoint 13 complete when:** `docker-compose up --build` starts the entire stack. CI pipeline runs on push. All tests pass.

---

## Final Endpoint Summary

When you're done, your API should expose something like this:

```
Auth:
  POST   /api/v1/auth/register
  POST   /api/v1/auth/login

Drivers:
  GET    /api/v1/drivers
  GET    /api/v1/drivers/{id}
  POST   /api/v1/drivers
  PUT    /api/v1/drivers/{id}
  PATCH  /api/v1/drivers/{id}/location
  PATCH  /api/v1/drivers/{id}/status
  DELETE /api/v1/drivers/{id}

Merchants:
  GET    /api/v1/merchants
  GET    /api/v1/merchants/{id}
  POST   /api/v1/merchants
  PUT    /api/v1/merchants/{id}
  DELETE /api/v1/merchants/{id}

Orders:
  GET    /api/v1/orders
  GET    /api/v1/orders/{id}
  POST   /api/v1/orders
  PATCH  /api/v1/orders/{id}/assign
  PATCH  /api/v1/orders/{id}/pickup
  PATCH  /api/v1/orders/{id}/deliver
  PATCH  /api/v1/orders/{id}/fail
  PATCH  /api/v1/orders/{id}/cancel

Zones:
  GET    /api/v1/zones
  POST   /api/v1/zones
  POST   /api/v1/zones/connections

Routing:
  GET    /api/v1/routing/shortest-path?from={zoneId}&to={zoneId}
  POST   /api/v1/routing/optimize-route

Dispatch:
  POST   /api/v1/dispatch/optimize-load

Analytics:
  GET    /api/v1/analytics/drivers/{id}/stats
  GET    /api/v1/analytics/orders/summary

System:
  GET    /health
```

---

## What To Study In Parallel

As you build each checkpoint, read the docs for the tool you're using:

| Checkpoint | Read About |
|---|---|
| 1 | ASP.NET Core fundamentals, DI lifetime (Singleton vs Scoped vs Transient) |
| 2 | EF Core relationships, Fluent API, migration strategies |
| 3 | MediatR docs, CQRS pattern, Repository pattern pros/cons |
| 4 | JWT anatomy, Claims-based auth, OWASP API security |
| 5 | State machine pattern, domain events vs integration events |
| 6 | Dijkstra's algorithm, graph representations, TSP heuristics |
| 7 | BackgroundService lifecycle, scoped services in singletons |
| 8 | Kafka fundamentals (topics, partitions, consumer groups, offsets) |
| 9 | Redis data structures, distributed locking pitfalls, cache-aside pattern |
| 10 | Polly policies, circuit breaker states, IHttpClientFactory lifecycle |
| 11 | Knapsack problem, sliding window technique, rate limiting algorithms |
| 12 | Testing pyramid, WebApplicationFactory, mocking strategies |
| 13 | Docker multi-stage builds, container networking, CI/CD basics |

Good luck! 🚀