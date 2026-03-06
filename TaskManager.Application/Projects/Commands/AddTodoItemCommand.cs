using MediatR;
using TaskManager.Application.Common;
using TaskManager.Application.Interfaces;
using TaskManager.Application.TodoItems.DTOs;
using TaskManager.Domain.Common;
using TaskManager.Domain.Enums;

namespace TaskManager.Application.Projects.Commands
{
    public record AddTodoItemCommand(
       Guid ProjectId,
       Guid UserId,
       Guid? AssigneeId,

       string Title,
       string? Description,

       DateTime? DueDate,
       Priority? Priority

   ) : IRequest<Result<TodoItemEntry>>, ICacheInvalidator
    {
        public string[] Keys => GetKeys().ToArray();

        private IEnumerable<string> GetKeys()
        {
            yield return CacheKeys.ProjectDetailedViews(UserId, ProjectId);

            yield return CacheKeys.ProjectTiles(UserId);

            if (AssigneeId is not null && AssigneeId.HasValue && AssigneeId != UserId)
            {
                yield return CacheKeys.AssignedTodoItems(AssigneeId ?? Guid.Empty); 
            }
        }
    }
}
