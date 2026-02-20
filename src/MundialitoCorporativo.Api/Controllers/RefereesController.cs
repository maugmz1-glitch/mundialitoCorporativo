using Asp.Versioning;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using MundialitoCorporativo.Api;
using MundialitoCorporativo.Application.Common;
using MundialitoCorporativo.Application.Referees.Commands;
using MundialitoCorporativo.Application.Referees.Queries;

namespace MundialitoCorporativo.Api.Controllers;

[ApiController]
[Route("api/v{version:apiVersion}/[controller]")]
[Route("api/[controller]")]
[ApiVersion("1.0")]
public class RefereesController : ControllerBase
{
    private readonly IMediator _mediator;

    public RefereesController(IMediator mediator) => _mediator = mediator;

    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(RefereeDto), 200)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new GetRefereeByIdQuery(id), cancellationToken);
        if (!result.IsSuccess) return result.ToActionResult();
        if (result.Data == null) return NotFound();
        return Ok(result.Data);
    }

    [HttpGet]
    [ProducesResponseType(typeof(PagedResponse<RefereeListItemDto>), 200)]
    public async Task<IActionResult> GetList([FromQuery] GetRefereesQuery query, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(query, cancellationToken);
        if (!result.IsSuccess) return result.ToActionResult();
        var pr = result.Data!;
        return Ok(new PagedResponse<RefereeListItemDto>(pr.Data, pr.PageNumber, pr.PageSize, pr.TotalRecords, pr.TotalPages));
    }

    [HttpPost]
    [ProducesResponseType(typeof(RefereeDto), 201)]
    [ProducesResponseType(400)]
    public async Task<IActionResult> Create([FromBody] CreateRefereeRequest request, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new CreateRefereeCommand(request.FirstName, request.LastName, request.LicenseNumber), cancellationToken);
        if (!result.IsSuccess) return result.ToActionResult();
        return CreatedAtAction(nameof(GetById), new { id = result.Data!.Id }, result.Data);
    }

    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(RefereeDto), 200)]
    [ProducesResponseType(400)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateRefereeRequest request, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new UpdateRefereeCommand(id, request.FirstName, request.LastName, request.LicenseNumber), cancellationToken);
        if (!result.IsSuccess) return result.ToActionResult();
        return Ok(result.Data);
    }

    [HttpDelete("{id:guid}")]
    [ProducesResponseType(204)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new DeleteRefereeCommand(id), cancellationToken);
        if (!result.IsSuccess) return result.ToActionResult();
        return NoContent();
    }
}

public record CreateRefereeRequest(string FirstName, string LastName, string? LicenseNumber);
public record UpdateRefereeRequest(string FirstName, string LastName, string? LicenseNumber);
