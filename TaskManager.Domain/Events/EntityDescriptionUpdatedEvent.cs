using TaskManager.Domain.Interfaces;

namespace TaskManager.Domain.Events
{
    public record EntityDescriptionUpdatedEvent(Guid id) : IDomainEvent
    {
        public DateTime OccurredOn { get; } = DateTime.UtcNow;
    }
}