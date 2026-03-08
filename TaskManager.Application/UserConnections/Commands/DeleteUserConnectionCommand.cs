using MediatR;
using TaskManager.Application.Common;
using TaskManager.Application.Interfaces;
using TaskManager.Domain.Common;

namespace TaskManager.Application.UserConnections.Commands
{
    public record DeleteUserConnectionCommand(
       Guid UserId,
       Guid ConnectionId) : IRequest<Result>, ICacheInvalidator
    {
        public string[] Keys => [CacheKeys.Connections(UserId)];
};
}
