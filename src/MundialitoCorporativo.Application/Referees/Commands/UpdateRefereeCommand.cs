using MundialitoCorporativo.Application.Common;
using MundialitoCorporativo.Application.Referees.Queries;

namespace MundialitoCorporativo.Application.Referees.Commands;

public record UpdateRefereeCommand(Guid Id, string FirstName, string LastName, string? LicenseNumber) : IRequest<RefereeDto>;
