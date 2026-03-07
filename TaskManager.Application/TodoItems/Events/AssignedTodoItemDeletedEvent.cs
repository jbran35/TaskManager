using MediatR;

namespace TaskManager.Application.TodoItems.Events
{
    public record AssignedTodoItemDeletedEvent(
        Guid? AssigneeId) : INotification;
}
