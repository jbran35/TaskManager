using MediatR;

namespace TaskManager.Application.UserConnections.Events
{
    public record UserRemovedFromGoupEvent(Guid? AssigneeId) : INotification;
    
}
