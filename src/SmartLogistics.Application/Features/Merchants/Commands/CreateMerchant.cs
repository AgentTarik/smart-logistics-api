using MediatR;
using NetTopologySuite.Geometries;
using SmartLogistics.Application.DTOs;
using SmartLogistics.Domain.Entities;
using SmartLogistics.Domain.Interfaces;

namespace SmartLogistics.Application.Features.Merchants.Commands;

public record CreateMerchantCommand(CreateMerchantRequest Request) : IRequest<MerchantResponse>;

public class CreateMerchantHandler : IRequestHandler<CreateMerchantCommand, MerchantResponse>
{
    private readonly IRepository<Merchant> _repository;

    public CreateMerchantHandler(IRepository<Merchant> repository)
    {
        _repository = repository;
    }

    public async Task<MerchantResponse> Handle(CreateMerchantCommand command, CancellationToken ct)
    {
        var merchant = new Merchant
        {
            Name = command.Request.Name,
            Email = command.Request.Email,
            Address = command.Request.Address,
            Location = new Point(command.Request.Longitude, command.Request.Latitude) { SRID = 4326 }
        };

        await _repository.AddAsync(merchant, ct);

        return new MerchantResponse(
            merchant.Id, merchant.Name, merchant.Email, merchant.ApiKey,
            merchant.Address,
            merchant.Location?.Y, merchant.Location?.X,
            merchant.CreatedAt
        );
    }
}
