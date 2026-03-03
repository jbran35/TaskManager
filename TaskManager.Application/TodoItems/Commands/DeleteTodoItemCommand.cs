using MediatR;
using TaskManager.Domain.Common;

namespace TaskManager.Application.TodoItems.Commands
{
    public record DeleteTodoItemCommand(
    Guid UserId,
    Guid TodoItemId

    ) : IRequest<Result>;
}


