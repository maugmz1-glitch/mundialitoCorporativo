using Asp.Versioning;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using MundialitoCorporativo.Api;
using MundialitoCorporativo.Application.Standings.Queries;

namespace MundialitoCorporativo.Api.Controllers;

[ApiController]
[Route("api/v{version:apiVersion}/[controller]")]
[Route("api/[controller]")]
[ApiVersion("1.0")]
public class StandingsController : ControllerBase
{
    private readonly IMediator _mediator;

    public StandingsController(IMediator mediator) => _mediator = mediator;

    [HttpGet]
    [ProducesResponseType(200)]
    [ProducesResponseType(400)]
    public async Task<IActionResult> Get(CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new GetStandingsQuery(), cancellationToken);
        if (!result.IsSuccess) return result.ToActionResult();
        return Ok(result.Data);
    }

    [HttpGet("top-scorers")]
    [ProducesResponseType(200)]
    [ProducesResponseType(400)]
    public async Task<IActionResult> GetTopScorers([FromQuery] int? limit, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new GetTopScorersQuery(limit ?? 10), cancellationToken);
        if (!result.IsSuccess) return result.ToActionResult();
        return Ok(result.Data);
    }
}
