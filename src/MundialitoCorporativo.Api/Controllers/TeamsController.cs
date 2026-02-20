using Asp.Versioning;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using MundialitoCorporativo.Api;
using MundialitoCorporativo.Application.Common;
using MundialitoCorporativo.Application.Teams.Commands;
using MundialitoCorporativo.Application.Teams.Queries;

namespace MundialitoCorporativo.Api.Controllers;

[ApiController]
[Route("api/v{version:apiVersion}/[controller]")]
[Route("api/[controller]")]
[ApiVersion("1.0")]
public class TeamsController : ControllerBase
{
    private readonly IMediator _mediator;

    public TeamsController(IMediator mediator) => _mediator = mediator;

    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(TeamDto), 200)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new GetTeamByIdQuery(id), cancellationToken);
        if (!result.IsSuccess) return result.ToActionResult();
        if (result.Data == null) return NotFound();
        return Ok(result.Data);
    }

    [HttpGet]
    [ProducesResponseType(typeof(PagedResponse<TeamListItemDto>), 200)]
    public async Task<IActionResult> GetList([FromQuery] GetTeamsQuery query, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(query, cancellationToken);
        if (!result.IsSuccess) return result.ToActionResult();
        var pr = result.Data!;
        return Ok(new PagedResponse<TeamListItemDto>(pr.Data, pr.PageNumber, pr.PageSize, pr.TotalRecords, pr.TotalPages));
    }

    [HttpPost]
    [ProducesResponseType(typeof(TeamDto), 201)]
    [ProducesResponseType(400)]
    [ProducesResponseType(409)]
    public async Task<IActionResult> Create([FromBody] CreateTeamRequest request, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new CreateTeamCommand(request.Name, request.LogoUrl), cancellationToken);
        if (!result.IsSuccess) return result.ToActionResult();
        return CreatedAtAction(nameof(GetById), new { id = result.Data!.Id }, result.Data);
    }

    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(TeamDto), 200)]
    [ProducesResponseType(400)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateTeamRequest request, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new UpdateTeamCommand(id, request.Name, request.LogoUrl), cancellationToken);
        if (!result.IsSuccess) return result.ToActionResult();
        return Ok(result.Data);
    }

    [HttpDelete("{id:guid}")]
    [ProducesResponseType(204)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new DeleteTeamCommand(id), cancellationToken);
        if (!result.IsSuccess) return result.ToActionResult();
        return NoContent();
    }
}

public record CreateTeamRequest(string Name, string? LogoUrl);
public record UpdateTeamRequest(string Name, string? LogoUrl);
