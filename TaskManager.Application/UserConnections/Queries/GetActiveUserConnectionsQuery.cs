using MediatR;
using TaskManager.Application.UserConnections.DTOs;
using TaskManager.Domain.Common;

namespace TaskManager.Application.UserConnections.Queries
{
    public record GetActiveUserConnectionsQuery(Guid UserId) : IRequest<Result<IEnumerable<UserConnectionDto>>>; 
}
