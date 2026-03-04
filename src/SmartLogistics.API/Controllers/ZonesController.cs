using MediatR;
using Microsoft.AspNetCore.Mvc;
using SmartLogistics.Application.DTOs;
using SmartLogistics.Application.Features.Zones.Commands;
using SmartLogistics.Application.Features.Zones.Queries;

namespace SmartLogistics.API.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
public class ZonesController : ControllerBase
{
    private readonly IMediator _mediator;

    public ZonesController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll(CancellationToken ct)
    {
        var result = await _mediator.Send(new GetAllZonesQuery(), ct);
        return Ok(result);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id, CancellationToken ct)
    {
        var result = await _mediator.Send(new GetZoneByIdQuery(id), ct);
        return result is null ? NotFound() : Ok(result);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateZoneRequest request, CancellationToken ct)
    {
        var result = await _mediator.Send(new CreateZoneCommand(request), ct);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateZoneRequest request, CancellationToken ct)
    {
        await _mediator.Send(new UpdateZoneCommand(id, request), ct);
        return NoContent();
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
    {
        await _mediator.Send(new DeleteZoneCommand(id), ct);
        return NoContent();
    }
}
