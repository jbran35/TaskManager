using MediatR;
using TaskManager.Application.UserConnections.DTOs;
using TaskManager.Domain.Common;

namespace TaskManager.Application.UserConnections.Commands
{
    public record CreateUserConnectionCommand(
        Guid UserId, 
        Guid AssigneeId
        ) : IRequest<Result<UserConnectionDto>>;
}
