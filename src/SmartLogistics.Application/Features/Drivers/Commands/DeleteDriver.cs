using MediatR;
using SmartLogistics.Domain.Entities;
using SmartLogistics.Domain.Interfaces;

namespace SmartLogistics.Application.Features.Drivers.Commands;

public record DeleteDriverCommand(Guid Id) : IRequest<Unit>;

public class DeleteDriverHandler : IRequestHandler<DeleteDriverCommand, Unit>
{
    private readonly IRepository<Driver> _repository;

    public DeleteDriverHandler(IRepository<Driver> repository)
    {
        _repository = repository;
    }

    public async Task<Unit> Handle(DeleteDriverCommand command, CancellationToken ct)
    {
        var driver = await _repository.GetByIdAsync(command.Id, ct)
            ?? throw new KeyNotFoundException($"Driver with ID {command.Id} not found");

        await _repository.DeleteAsync(driver, ct);
        return Unit.Value;
    }
}
