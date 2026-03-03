using MediatR;
using TaskManager.Domain.Common;

namespace TaskManager.Application.TodoItems.Commands
{
    public record AssignTodoItemCommand(

        Guid UserId,
        Guid ProjectId,
        Guid TodoItemId,
        Guid AssigneeId) : IRequest<Result>;
}
