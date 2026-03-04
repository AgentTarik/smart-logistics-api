using MediatR;
using NetTopologySuite.Geometries;
using SmartLogistics.Application.DTOs;
using SmartLogistics.Domain.Entities;
using SmartLogistics.Domain.Interfaces;

namespace SmartLogistics.Application.Features.Zones.Commands;

public record CreateZoneCommand(CreateZoneRequest Request) : IRequest<ZoneResponse>;

public class CreateZoneHandler : IRequestHandler<CreateZoneCommand, ZoneResponse>
{
    private readonly IRepository<Zone> _repository;

    public CreateZoneHandler(IRepository<Zone> repository)
    {
        _repository = repository;
    }

    public async Task<ZoneResponse> Handle(CreateZoneCommand command, CancellationToken ct)
    {
        var coordinates = command.Request.BoundaryCoordinates
            .Select(c => new Coordinate(c[0], c[1]))
            .ToArray();

        // Ensure the polygon is closed
        if (coordinates.First() != coordinates.Last())
        {
            coordinates = coordinates.Append(coordinates.First()).ToArray();
        }

        var zone = new Zone
        {
            Name = command.Request.Name,
            BaseDeliveryCost = command.Request.BaseDeliveryCost,
            Boundary = new Polygon(new LinearRing(coordinates)) { SRID = 4326 }
        };

        await _repository.AddAsync(zone, ct);

        return new ZoneResponse(
            zone.Id, zone.Name, zone.BaseDeliveryCost, zone.CreatedAt
        );
    }
}
