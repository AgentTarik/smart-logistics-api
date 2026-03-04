using MediatR;
using SmartLogistics.Domain.Entities;
using SmartLogistics.Domain.Interfaces;

namespace SmartLogistics.Application.Features.Zones.Commands;

public record DeleteZoneCommand(Guid Id) : IRequest<Unit>;

public class DeleteZoneHandler : IRequestHandler<DeleteZoneCommand, Unit>
{
    private readonly IRepository<Zone> _repository;

    public DeleteZoneHandler(IRepository<Zone> repository)
    {
        _repository = repository;
    }

    public async Task<Unit> Handle(DeleteZoneCommand command, CancellationToken ct)
    {
        var zone = await _repository.GetByIdAsync(command.Id, ct)
            ?? throw new KeyNotFoundException($"Zone with ID {command.Id} not found");

        await _repository.DeleteAsync(zone, ct);
        return Unit.Value;
    }
}
