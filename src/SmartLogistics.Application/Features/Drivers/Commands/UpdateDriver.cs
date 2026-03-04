using MediatR;
using SmartLogistics.Application.DTOs;
using SmartLogistics.Domain.Entities;
using SmartLogistics.Domain.Interfaces;

namespace SmartLogistics.Application.Features.Drivers.Commands;

public record UpdateDriverCommand(Guid Id, UpdateDriverRequest Request) : IRequest<Unit>;

public class UpdateDriverHandler : IRequestHandler<UpdateDriverCommand, Unit>
{
    private readonly IRepository<Driver> _repository;

    public UpdateDriverHandler(IRepository<Driver> repository)
    {
        _repository = repository;
    }

    public async Task<Unit> Handle(UpdateDriverCommand command, CancellationToken ct)
    {
        var driver = await _repository.GetByIdAsync(command.Id, ct)
            ?? throw new KeyNotFoundException($"Driver with ID {command.Id} not found");

        driver.Name = command.Request.Name;
        driver.PhoneNumber = command.Request.PhoneNumber;
        driver.LicensePlate = command.Request.LicensePlate;
        driver.MaxCargoWeightKg = command.Request.MaxCargoWeightKg;
        driver.MaxCargoVolumeM3 = command.Request.MaxCargoVolumeM3;

        await _repository.UpdateAsync(driver, ct);
        return Unit.Value;
    }
}
