using MediatR;

namespace TaskManager.Application.Projects.Events
{
    public record AssignedTodoItemCreatedEvent(
        Guid? AssigneeId) : INotification;
}
