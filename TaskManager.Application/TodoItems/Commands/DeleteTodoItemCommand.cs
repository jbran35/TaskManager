using MediatR;
using TaskManager.Application.Common;
using TaskManager.Application.Interfaces;
using TaskManager.Domain.Common;

namespace TaskManager.Application.TodoItems.Commands
{
    public record DeleteTodoItemCommand(
    Guid UserId,
    Guid TodoItemId

    ) : IRequest<Result>, ICacheInvalidator
    {
        public string[] Keys => [CacheKeys.ProjectTiles(UserId)];
    }
}


