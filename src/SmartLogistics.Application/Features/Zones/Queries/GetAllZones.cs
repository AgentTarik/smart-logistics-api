using MediatR;
using SmartLogistics.Application.DTOs;
using SmartLogistics.Domain.Entities;
using SmartLogistics.Domain.Interfaces;

namespace SmartLogistics.Application.Features.Zones.Queries;

public record GetAllZonesQuery() : IRequest<IReadOnlyList<ZoneResponse>>;

public class GetAllZonesHandler : IRequestHandler<GetAllZonesQuery, IReadOnlyList<ZoneResponse>>
{
    private readonly IRepository<Zone> _repository;

    public GetAllZonesHandler(IRepository<Zone> repository)
    {
        _repository = repository;
    }

    public async Task<IReadOnlyList<ZoneResponse>> Handle(GetAllZonesQuery query, CancellationToken ct)
    {
        var zones = await _repository.GetAllAsync(ct);

        return zones.Select(z => new ZoneResponse(
            z.Id, z.Name, z.BaseDeliveryCost, z.CreatedAt
        )).ToList();
    }
}
