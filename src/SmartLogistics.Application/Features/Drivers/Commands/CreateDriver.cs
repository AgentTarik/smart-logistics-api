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