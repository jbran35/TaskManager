using MediatR;
using TaskManager.Application.Common;
using TaskManager.Application.Interfaces;
using TaskManager.Domain.Common;

namespace TaskManager.Application.TodoItems.Commands
{
    public record AssignTodoItemCommand(
        
        Guid UserId,
        Guid ProjectId,
        Guid TodoItemId,
        Guid AssigneeId) : IRequest<Result>, ICacheInvalidator
    {
        public string[] Keys => GetKeys().ToArray();
        private IEnumerable<string> GetKeys()
        {
            yield return CacheKeys.ProjectDetailedViews(UserId, ProjectId);

            if(AssigneeId != Guid.Empty && AssigneeId != UserId)
            {
                yield return CacheKeys.AssignedTodoItems(AssigneeId); 
            }
        }
    }
}
