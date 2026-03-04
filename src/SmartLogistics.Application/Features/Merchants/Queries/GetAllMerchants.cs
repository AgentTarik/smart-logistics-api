using MediatR;
using SmartLogistics.Application.DTOs;
using SmartLogistics.Domain.Entities;
using SmartLogistics.Domain.Interfaces;

namespace SmartLogistics.Application.Features.Merchants.Queries;

public record GetAllMerchantsQuery() : IRequest<IReadOnlyList<MerchantResponse>>;

public class GetAllMerchantsHandler : IRequestHandler<GetAllMerchantsQuery, IReadOnlyList<MerchantResponse>>
{
    private readonly IRepository<Merchant> _repository;

    public GetAllMerchantsHandler(IRepository<Merchant> repository)
    {
        _repository = repository;
    }

    public async Task<IReadOnlyList<MerchantResponse>> Handle(GetAllMerchantsQuery query, CancellationToken ct)
    {
        var merchants = await _repository.GetAllAsync(ct);

        return merchants.Select(m => new MerchantResponse(
            m.Id, m.Name, m.Email, m.ApiKey,
            m.Address,
            m.Location?.Y, m.Location?.X,
            m.CreatedAt
        )).ToList();
    }
}
