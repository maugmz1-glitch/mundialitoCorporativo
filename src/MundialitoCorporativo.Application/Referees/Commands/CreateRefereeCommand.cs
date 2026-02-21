using MundialitoCorporativo.Application.Common;
using MundialitoCorporativo.Application.Referees.Queries;

namespace MundialitoCorporativo.Application.Referees.Commands;

public record CreateRefereeCommand(string FirstName, string LastName, string? LicenseNumber) : IRequest<RefereeDto>;
