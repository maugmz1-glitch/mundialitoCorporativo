using MediatR;
using Microsoft.EntityFrameworkCore;
using MundialitoCorporativo.Application.Common;
using MundialitoCorporativo.Application.Interfaces;
using MundialitoCorporativo.Application.Teams.Queries;
using MundialitoCorporativo.Domain.Common;
using MundialitoCorporativo.Domain.Entities;

namespace MundialitoCorporativo.Application.Teams.Commands;

/// <summary>
/// Handler del comando CreateTeam. Ejemplo de flujo CQRS de escritura:
/// validación con Result.Failure (sin excepciones) y persistencia con EF Core (IAppDbContext).
/// </summary>
public class CreateTeamCommandHandler : IRequestHandler<CreateTeamCommand, Result<TeamDto>>
{
    private readonly IAppDbContext _db;

    public CreateTeamCommandHandler(IAppDbContext db) => _db = db;

    public async Task<Result<TeamDto>> Handle(CreateTeamCommand request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.Name))
            return Result.Failure<TeamDto>("Team name is required.", ErrorCodes.Validation);

        var name = request.Name.Trim();
        var exists = await _db.Teams.AnyAsync(t => t.Name == name, cancellationToken);
        if (exists)
            return Result.Failure<TeamDto>("A team with this name already exists.", ErrorCodes.Conflict);

        var team = new Team
        {
            Id = Guid.NewGuid(),
            Name = name,
            LogoUrl = request.LogoUrl?.Trim(),
            CreatedAtUtc = DateTime.UtcNow
        };
        // Escritura exclusiva con EF Core (DbContext).
        _db.Teams.Add(team);
        await _db.SaveChangesAsync(cancellationToken);
        return Result.Success(new TeamDto(team.Id, team.Name, team.LogoUrl, team.CreatedAtUtc));
    }
}
