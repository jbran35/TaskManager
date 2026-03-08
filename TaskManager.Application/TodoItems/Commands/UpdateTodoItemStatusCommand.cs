using MediatR;
using TaskManager.Application.Common;
using TaskManager.Application.Interfaces;
using TaskManager.Application.TodoItems.DTOs;
using TaskManager.Domain.Common;

namespace TaskManager.Application.TodoItems.Commands
{
    public record UpdateTodoItemStatusCommand(
        Guid UserId,
        Guid TodoItemId
        ) : IRequest<Result<TodoItemEntry>>, ICacheInvalidator
    {
        public string[] Keys => [CacheKeys.ProjectTiles(UserId)];
    }
}
