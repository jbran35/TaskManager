using MediatR;
using TaskManager.Application.TodoItems.DTOs;
using TaskManager.Domain.Common;
using TaskManager.Domain.Enums;
using TaskManager.Domain.Interfaces;

namespace TaskManager.Application.TodoItems.Commands
{
    public record UpdateTodoItemCommand(
        Guid UserId,
        Guid ProjectId,
        Guid TodoItemId,
        Guid? AssigneeId,

        string? NewTitle,
        string? NewDescription,

        Priority? NewPriority,
        DateTime? NewDueDate
        ) : IRequest<Result<TodoItemEntry>>;
}
