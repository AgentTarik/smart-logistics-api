using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Serilog;
using SmartLogistics.API.Middleware;
using SmartLogistics.Application.Behaviors;
using SmartLogistics.Application.Features.Drivers.Commands;
using SmartLogistics.Application.Validators;
using SmartLogistics.Domain.Interfaces;
using SmartLogistics.Infrastructure.Data;
using SmartLogistics.Infrastructure.Repositories;

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
    builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(
        builder.Configuration.GetConnectionString("DefaultConnection"),
        x => x.UseNetTopologySuite()
    ));
    builder.Services.AddEndpointsApiExplorer();
    builder.Services.AddSwaggerGen();

    // MediatR
    builder.Services.AddMediatR(cfg =>
        cfg.RegisterServicesFromAssembly(typeof(CreateDriverCommand).Assembly));

    // FluentValidation
    builder.Services.AddValidatorsFromAssembly(typeof(CreateDriverValidator).Assembly);
    builder.Services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));

    // Repositories
    builder.Services.AddScoped(typeof(IRepository<>), typeof(Repository<>));

    var app = builder.Build();

    // Middleware pipeline
    app.UseMiddleware<ExceptionMiddleware>();
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
