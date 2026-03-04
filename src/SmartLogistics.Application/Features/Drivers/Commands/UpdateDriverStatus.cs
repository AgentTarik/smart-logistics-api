using MediatR;
using SmartLogistics.Domain.Entities;
using SmartLogistics.Domain.Interfaces;

namespace SmartLogistics.Application.Features.Drivers.Commands;

public record UpdateDriverStatusCommand(Guid Id, DriverStatus Status) : IRequest<Unit>;

public class UpdateDriverStatusHandler : IRequestHandler<UpdateDriverStatusCommand, Unit>
{
    private readonly IRepository<Driver> _repository;

    public UpdateDriverStatusHandler(IRepository<Driver> repository)
    {
        _repository = repository;
    }

    public async Task<Unit> Handle(UpdateDriverStatusCommand command, CancellationToken ct)
    {
        var driver = await _repository.GetByIdAsync(command.Id, ct)
            ?? throw new KeyNotFoundException($"Driver with ID {command.Id} not found");

        driver.Status = command.Status;

        await _repository.UpdateAsync(driver, ct);
        return Unit.Value;
    }
}
