using MediatR;
using TaskManager.Application.TodoItems.DTOs;
using TaskManager.Domain.Common;

namespace TaskManager.Application.TodoItems.Queries
{
    public record GetAssignedTodoItemsQuery(Guid UserId) : IRequest<Result<List<TodoItemEntry>>>; 
}
