using MediatR;
using NetTopologySuite.Geometries;
using SmartLogistics.Application.DTOs;
using SmartLogistics.Domain.Entities;
using SmartLogistics.Domain.Interfaces;

namespace SmartLogistics.Application.Features.Drivers.Commands;

public record UpdateDriverLocationCommand(Guid Id, UpdateDriverLocationRequest Request) : IRequest<Unit>;

public class UpdateDriverLocationHandler : IRequestHandler<UpdateDriverLocationCommand, Unit>
{
    private readonly IRepository<Driver> _repository;

    public UpdateDriverLocationHandler(IRepository<Driver> repository)
    {
        _repository = repository;
    }

    public async Task<Unit> Handle(UpdateDriverLocationCommand command, CancellationToken ct)
    {
        var driver = await _repository.GetByIdAsync(command.Id, ct)
            ?? throw new KeyNotFoundException($"Driver with ID {command.Id} not found");

        driver.CurrentLocation = new Point(command.Request.Longitude, command.Request.Latitude) { SRID = 4326 };

        await _repository.UpdateAsync(driver, ct);
        return Unit.Value;
    }
}
