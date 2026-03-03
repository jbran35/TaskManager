using MediatR;
using TaskManager.Application.TodoItems.DTOs;
using TaskManager.Domain.Common;

namespace TaskManager.Application.TodoItems.Queries
{
    public record GetTodoItemDetailedViewQuery (Guid Id, Guid UserId) : IRequest<Result<TodoItemEntry>>;
}
