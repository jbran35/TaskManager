using MediatR;

namespace TaskManager.Application.TodoItems.Events
{
    public record TodoItemAssignmentChangedEvent(
        Guid? OldAssigneeId,
        Guid? NewAssigneeId) : INotification;

}
