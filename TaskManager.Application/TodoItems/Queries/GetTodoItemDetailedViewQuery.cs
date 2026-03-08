using MediatR;
using TaskManager.Application.TodoItems.DTOs;
using TaskManager.Domain.Common;

namespace TaskManager.Application.TodoItems.Queries
{
    public record GetTodoItemDetailedViewQuery : IRequest<Result<TodoItemEntry>>
    {
        public Guid UserId { get; set; }
        public Guid TodoItemId { get; set; }
        public Guid ProjectId { get; set; }

    }
}
