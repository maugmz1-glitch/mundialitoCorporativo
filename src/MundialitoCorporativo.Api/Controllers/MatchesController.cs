using Asp.Versioning;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using MundialitoCorporativo.Api;
using MundialitoCorporativo.Application.Common;
using MundialitoCorporativo.Application.Matches.Commands;
using MundialitoCorporativo.Application.Matches.Queries;

namespace MundialitoCorporativo.Api.Controllers;

[ApiController]
[Route("api/v{version:apiVersion}/[controller]")]
[Route("api/[controller]")]
[ApiVersion("1.0")]
public class MatchesController : ControllerBase
{
    private readonly IMediator _mediator;

    public MatchesController(IMediator mediator) => _mediator = mediator;

    [HttpGet("{id:guid}")]
    [ProducesResponseType(200)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new GetMatchByIdQuery(id), cancellationToken);
        if (!result.IsSuccess) return result.ToActionResult();
        if (result.Data == null) return NotFound();
        return Ok(result.Data);
    }

    [HttpGet]
    [ProducesResponseType(200)]
    public async Task<IActionResult> GetList([FromQuery] GetMatchesQuery query, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(query, cancellationToken);
        if (!result.IsSuccess) return result.ToActionResult();
        var pr = result.Data!;
        return Ok(new PagedResponse<MatchListItemDto>(pr.Data, pr.PageNumber, pr.PageSize, pr.TotalRecords, pr.TotalPages));
    }

    [HttpPost]
    [ProducesResponseType(201)]
    [ProducesResponseType(400)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> Create([FromBody] CreateMatchRequest request, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new CreateMatchCommand(request.HomeTeamId, request.AwayTeamId, request.RefereeId, request.ScheduledAtUtc, request.Venue), cancellationToken);
        if (!result.IsSuccess) return result.ToActionResult();
        return CreatedAtAction(nameof(GetById), new { id = result.Data!.Id }, result.Data);
    }

    [HttpPut("{id:guid}")]
    [ProducesResponseType(200)]
    [ProducesResponseType(400)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateMatchRequest request, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new UpdateMatchCommand(id, request.HomeTeamId, request.AwayTeamId, request.RefereeId, request.ScheduledAtUtc, request.Venue, request.Status), cancellationToken);
        if (!result.IsSuccess) return result.ToActionResult();
        return Ok(result.Data);
    }

    [HttpPatch("{id:guid}/result")]
    [ProducesResponseType(200)]
    [ProducesResponseType(400)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> SetResult(Guid id, [FromBody] SetMatchResultRequest request, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new SetMatchResultCommand(id, request.HomeScore, request.AwayScore), cancellationToken);
        if (!result.IsSuccess) return result.ToActionResult();
        return Ok(result.Data);
    }

    [HttpPost("{id:guid}/cards")]
    [ProducesResponseType(201)]
    [ProducesResponseType(400)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> AddCard(Guid id, [FromBody] AddMatchCardRequest request, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new AddMatchCardCommand(id, request.PlayerId, request.CardType, request.Minute), cancellationToken);
        if (!result.IsSuccess) return result.ToActionResult();
        return CreatedAtAction(nameof(GetById), new { id }, result.Data);
    }

    [HttpDelete("{id:guid}")]
    [ProducesResponseType(204)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new DeleteMatchCommand(id), cancellationToken);
        if (!result.IsSuccess) return result.ToActionResult();
        return NoContent();
    }

}

public record CreateMatchRequest(Guid HomeTeamId, Guid AwayTeamId, Guid? RefereeId, DateTime ScheduledAtUtc, string? Venue);
public record UpdateMatchRequest(Guid HomeTeamId, Guid AwayTeamId, Guid? RefereeId, DateTime ScheduledAtUtc, string? Venue, int Status);
public record SetMatchResultRequest(int HomeScore, int AwayScore);
public record AddMatchCardRequest(Guid PlayerId, int CardType, int Minute);
