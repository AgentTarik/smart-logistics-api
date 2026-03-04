using MediatR;
using NetTopologySuite.Geometries;
using SmartLogistics.Application.DTOs;
using SmartLogistics.Domain.Entities;
using SmartLogistics.Domain.Interfaces;

namespace SmartLogistics.Application.Features.Zones.Commands;

public record UpdateZoneCommand(Guid Id, UpdateZoneRequest Request) : IRequest<Unit>;

public class UpdateZoneHandler : IRequestHandler<UpdateZoneCommand, Unit>
{
    private readonly IRepository<Zone> _repository;

    public UpdateZoneHandler(IRepository<Zone> repository)
    {
        _repository = repository;
    }

    public async Task<Unit> Handle(UpdateZoneCommand command, CancellationToken ct)
    {
        var zone = await _repository.GetByIdAsync(command.Id, ct)
            ?? throw new KeyNotFoundException($"Zone with ID {command.Id} not found");

        zone.Name = command.Request.Name;
        zone.BaseDeliveryCost = command.Request.BaseDeliveryCost;

        if (command.Request.BoundaryCoordinates is not null)
        {
            var coordinates = command.Request.BoundaryCoordinates
                .Select(c => new Coordinate(c[0], c[1]))
                .ToArray();

            if (coordinates.First() != coordinates.Last())
            {
                coordinates = coordinates.Append(coordinates.First()).ToArray();
            }

            zone.Boundary = new Polygon(new LinearRing(coordinates)) { SRID = 4326 };
        }

        await _repository.UpdateAsync(zone, ct);
        return Unit.Value;
    }
}
