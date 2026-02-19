using MediatR;
using Microsoft.AspNetCore.Mvc;
using MundialitoCorporativo.Application.Common;
using MundialitoCorporativo.Application.Standings.Queries;

namespace MundialitoCorporativo.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class StandingsController : ControllerBase
{
    private readonly IMediator _mediator;

    public StandingsController(IMediator mediator) => _mediator = mediator;

    [HttpGet]
    [ProducesResponseType(200)]
    public async Task<IActionResult> Get(CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new GetStandingsQuery(), cancellationToken);
        if (!result.IsSuccess) return BadRequest(new { message = result.Message, code = result.ErrorCode });
        return Ok(result.Data);
    }

    [HttpGet("top-scorers")]
    [ProducesResponseType(200)]
    public async Task<IActionResult> GetTopScorers([FromQuery] int? limit, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new GetTopScorersQuery(limit ?? 10), cancellationToken);
        if (!result.IsSuccess) return BadRequest(new { message = result.Message, code = result.ErrorCode });
        return Ok(result.Data);
    }
}
