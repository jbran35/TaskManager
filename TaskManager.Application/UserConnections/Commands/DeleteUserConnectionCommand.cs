using MediatR;
using TaskManager.Domain.Common;

namespace TaskManager.Application.UserConnections.Commands
{
    public record DeleteUserConnectionCommand(
       Guid UserId,
       Guid ConnectionId) : IRequest<Result>;
}
