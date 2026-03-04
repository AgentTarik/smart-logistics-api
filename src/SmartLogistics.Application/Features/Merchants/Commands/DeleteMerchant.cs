using MediatR;
using SmartLogistics.Domain.Entities;
using SmartLogistics.Domain.Interfaces;

namespace SmartLogistics.Application.Features.Merchants.Commands;

public record DeleteMerchantCommand(Guid Id) : IRequest<Unit>;

public class DeleteMerchantHandler : IRequestHandler<DeleteMerchantCommand, Unit>
{
    private readonly IRepository<Merchant> _repository;

    public DeleteMerchantHandler(IRepository<Merchant> repository)
    {
        _repository = repository;
    }

    public async Task<Unit> Handle(DeleteMerchantCommand command, CancellationToken ct)
    {
        var merchant = await _repository.GetByIdAsync(command.Id, ct)
            ?? throw new KeyNotFoundException($"Merchant with ID {command.Id} not found");

        await _repository.DeleteAsync(merchant, ct);
        return Unit.Value;
    }
}
