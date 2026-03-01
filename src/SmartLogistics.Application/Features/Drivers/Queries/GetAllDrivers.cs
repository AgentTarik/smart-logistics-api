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