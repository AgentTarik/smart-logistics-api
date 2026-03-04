using MediatR;
using NetTopologySuite.Geometries;
using SmartLogistics.Application.DTOs;
using SmartLogistics.Domain.Entities;
using SmartLogistics.Domain.Interfaces;

namespace SmartLogistics.Application.Features.Merchants.Commands;

public record UpdateMerchantCommand(Guid Id, UpdateMerchantRequest Request) : IRequest<Unit>;

public class UpdateMerchantHandler : IRequestHandler<UpdateMerchantCommand, Unit>
{
    private readonly IRepository<Merchant> _repository;

    public UpdateMerchantHandler(IRepository<Merchant> repository)
    {
        _repository = repository;
    }

    public async Task<Unit> Handle(UpdateMerchantCommand command, CancellationToken ct)
    {
        var merchant = await _repository.GetByIdAsync(command.Id, ct)
            ?? throw new KeyNotFoundException($"Merchant with ID {command.Id} not found");

        merchant.Name = command.Request.Name;
        merchant.Address = command.Request.Address;
        merchant.Location = new Point(command.Request.Longitude, command.Request.Latitude) { SRID = 4326 };

        await _repository.UpdateAsync(merchant, ct);
        return Unit.Value;
    }
}
