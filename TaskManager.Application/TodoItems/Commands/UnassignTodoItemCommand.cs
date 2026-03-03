using MediatR;
using TaskManager.Domain.Common;

namespace TaskManager.Application.TodoItems.Commands
{
    public record UnassignTodoItemCommand(

        Guid UserId,
        Guid ProjectId,
        Guid TodoItemId) : IRequest<Result>;
}
