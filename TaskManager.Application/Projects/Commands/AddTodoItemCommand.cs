using MediatR;
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

    ) : IRequest<Result<TodoItemEntry>>;
}
