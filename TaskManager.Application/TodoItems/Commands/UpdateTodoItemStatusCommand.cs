using MediatR;
using TaskManager.Application.TodoItems.DTOs;
using TaskManager.Application.TodoItems.DTOs.Responses;
using TaskManager.Domain.Common;

namespace TaskManager.Application.TodoItems.Commands
{
    public record UpdateTodoItemStatusCommand(
        Guid UserId,
        Guid TodoItemId
        ) : IRequest<Result<TodoItemEntry>>;
}
