using MediatR;
using SmartLogistics.Application.DTOs;
using SmartLogistics.Domain.Entities;
using SmartLogistics.Domain.Interfaces;

namespace SmartLogistics.Application.Features.Zones.Queries;

public record GetZoneByIdQuery(Guid Id) : IRequest<ZoneResponse?>;

public class GetZoneByIdHandler : IRequestHandler<GetZoneByIdQuery, ZoneResponse?>
{
    private readonly IRepository<Zone> _repository;

    public GetZoneByIdHandler(IRepository<Zone> repository)
    {
        _repository = repository;
    }

    public async Task<ZoneResponse?> Handle(GetZoneByIdQuery query, CancellationToken ct)
    {
        var zone = await _repository.GetByIdAsync(query.Id, ct);
        if (zone is null) return null;

        return new ZoneResponse(
            zone.Id, zone.Name, zone.BaseDeliveryCost, zone.CreatedAt
        );
    }
}
