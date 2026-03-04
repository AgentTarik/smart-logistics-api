using MediatR;
using SmartLogistics.Application.DTOs;
using SmartLogistics.Domain.Entities;
using SmartLogistics.Domain.Interfaces;

namespace SmartLogistics.Application.Features.Drivers.Queries;

public record GetDriverByIdQuery(Guid Id) : IRequest<DriverResponse?>;

public class GetDriverByIdHandler : IRequestHandler<GetDriverByIdQuery, DriverResponse?>
{
    private readonly IRepository<Driver> _repository;

    public GetDriverByIdHandler(IRepository<Driver> repository)
    {
        _repository = repository;
    }

    public async Task<DriverResponse?> Handle(GetDriverByIdQuery query, CancellationToken ct)
    {
        var driver = await _repository.GetByIdAsync(query.Id, ct);
        if (driver is null) return null;

        return new DriverResponse(
            driver.Id, driver.Name, driver.Email, driver.PhoneNumber,
            driver.LicensePlate, driver.Status.ToString(),
            driver.CurrentLocation?.Y, driver.CurrentLocation?.X,
            driver.MaxCargoWeightKg, driver.MaxCargoVolumeM3,
            driver.CreatedAt
        );
    }
}
