using MediatR;
using Microsoft.AspNetCore.Mvc;
using SmartLogistics.Application.DTOs;
using SmartLogistics.Application.Features.Drivers.Commands;
using SmartLogistics.Application.Features.Drivers.Queries;

namespace SmartLogistics.API.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
public class DriversController : ControllerBase
{
    private readonly IMediator _mediator;

    public DriversController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll(CancellationToken ct)
    {
        var result = await _mediator.Send(new GetAllDriversQuery(), ct);
        return Ok(result);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id, CancellationToken ct)
    {
        var result = await _mediator.Send(new GetDriverByIdQuery(id), ct);
        return result is null ? NotFound() : Ok(result);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateDriverRequest request, CancellationToken ct)
    {
        var result = await _mediator.Send(new CreateDriverCommand(request), ct);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateDriverRequest request, CancellationToken ct)
    {
        await _mediator.Send(new UpdateDriverCommand(id, request), ct);
        return NoContent();
    }

    [HttpPatch("{id:guid}/location")]
    public async Task<IActionResult> UpdateLocation(Guid id, [FromBody] UpdateDriverLocationRequest request, CancellationToken ct)
    {
        await _mediator.Send(new UpdateDriverLocationCommand(id, request), ct);
        return NoContent();
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
    {
        await _mediator.Send(new DeleteDriverCommand(id), ct);
        return NoContent();
    }
}
