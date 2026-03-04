using MediatR;
using SmartLogistics.Application.DTOs;
using SmartLogistics.Domain.Entities;
using SmartLogistics.Domain.Interfaces;

namespace SmartLogistics.Application.Features.Merchants.Queries;

public record GetMerchantByIdQuery(Guid Id) : IRequest<MerchantResponse?>;

public class GetMerchantByIdHandler : IRequestHandler<GetMerchantByIdQuery, MerchantResponse?>
{
    private readonly IRepository<Merchant> _repository;

    public GetMerchantByIdHandler(IRepository<Merchant> repository)
    {
        _repository = repository;
    }

    public async Task<MerchantResponse?> Handle(GetMerchantByIdQuery query, CancellationToken ct)
    {
        var merchant = await _repository.GetByIdAsync(query.Id, ct);
        if (merchant is null) return null;

        return new MerchantResponse(
            merchant.Id, merchant.Name, merchant.Email, merchant.ApiKey,
            merchant.Address,
            merchant.Location?.Y, merchant.Location?.X,
            merchant.CreatedAt
        );
    }
}
