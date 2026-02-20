using Asp.Versioning;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using MundialitoCorporativo.Api;
using MundialitoCorporativo.Application.Common;
using MundialitoCorporativo.Application.Players.Commands;
using MundialitoCorporativo.Application.Players.Queries;

namespace MundialitoCorporativo.Api.Controllers;

[ApiController]
[Route("api/v{version:apiVersion}/[controller]")]
[Route("api/[controller]")]
[ApiVersion("1.0")]
public class PlayersController : ControllerBase
{
    private readonly IMediator _mediator;

    public PlayersController(IMediator mediator) => _mediator = mediator;

    [HttpGet("{id:guid}")]
    [ProducesResponseType(200)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new GetPlayerByIdQuery(id), cancellationToken);
        if (!result.IsSuccess) return result.ToActionResult();
        if (result.Data == null) return NotFound();
        return Ok(result.Data);
    }

    [HttpGet]
    [ProducesResponseType(200)]
    public async Task<IActionResult> GetList([FromQuery] GetPlayersQuery query, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(query, cancellationToken);
        if (!result.IsSuccess) return result.ToActionResult();
        var pr = result.Data!;
        return Ok(new PagedResponse<PlayerListItemDto>(pr.Data, pr.PageNumber, pr.PageSize, pr.TotalRecords, pr.TotalPages));
    }

    [HttpPost]
    [ProducesResponseType(201)]
    [ProducesResponseType(400)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> Create([FromBody] CreatePlayerRequest request, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new CreatePlayerCommand(request.TeamId, request.FirstName, request.LastName, request.JerseyNumber, request.Position), cancellationToken);
        if (!result.IsSuccess) return result.ToActionResult();
        return CreatedAtAction(nameof(GetById), new { id = result.Data!.Id }, result.Data);
    }

    [HttpPut("{id:guid}")]
    [ProducesResponseType(200)]
    [ProducesResponseType(400)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdatePlayerRequest request, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new UpdatePlayerCommand(id, request.TeamId, request.FirstName, request.LastName, request.JerseyNumber, request.Position), cancellationToken);
        if (!result.IsSuccess) return result.ToActionResult();
        return Ok(result.Data);
    }

    [HttpDelete("{id:guid}")]
    [ProducesResponseType(204)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new DeletePlayerCommand(id), cancellationToken);
        if (!result.IsSuccess) return result.ToActionResult();
        return NoContent();
    }

}

public record CreatePlayerRequest(Guid TeamId, string FirstName, string LastName, string? JerseyNumber, string? Position);
public record UpdatePlayerRequest(Guid TeamId, string FirstName, string LastName, string? JerseyNumber, string? Position);
