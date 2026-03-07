using MediatR;
using TaskManager.Application.Common;
using TaskManager.Application.Interfaces;
using TaskManager.Application.TodoItems.DTOs;
using TaskManager.Domain.Common;
using TaskManager.Domain.Enums;

namespace TaskManager.Application.TodoItems.Commands
{
    public record UpdateTodoItemCommand(
        Guid UserId,
        Guid ProjectId,
        Guid TodoItemId,
        Guid? AssigneeId,

        string? NewTitle,
        string? NewDescription,

        Priority? NewPriority,
        DateTime? NewDueDate
        ) : IRequest<Result<TodoItemEntry>>, ICacheInvalidator
    {
        public string[] Keys => GetKeys().ToArray();

        private IEnumerable<string> GetKeys()
        {
            yield return CacheKeys.ProjectTiles(UserId);
            yield return CacheKeys.ProjectDetailedViews(UserId, ProjectId); 
        }
    }
}
