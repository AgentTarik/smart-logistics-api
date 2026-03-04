using MediatR;
using Microsoft.AspNetCore.Mvc;
using SmartLogistics.Application.DTOs;
using SmartLogistics.Application.Features.Merchants.Commands;
using SmartLogistics.Application.Features.Merchants.Queries;

namespace SmartLogistics.API.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
public class MerchantsController : ControllerBase
{
    private readonly IMediator _mediator;

    public MerchantsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll(CancellationToken ct)
    {
        var result = await _mediator.Send(new GetAllMerchantsQuery(), ct);
        return Ok(result);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id, CancellationToken ct)
    {
        var result = await _mediator.Send(new GetMerchantByIdQuery(id), ct);
        return result is null ? NotFound() : Ok(result);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateMerchantRequest request, CancellationToken ct)
    {
        var result = await _mediator.Send(new CreateMerchantCommand(request), ct);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateMerchantRequest request, CancellationToken ct)
    {
        await _mediator.Send(new UpdateMerchantCommand(id, request), ct);
        return NoContent();
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
    {
        await _mediator.Send(new DeleteMerchantCommand(id), ct);
        return NoContent();
    }
}
