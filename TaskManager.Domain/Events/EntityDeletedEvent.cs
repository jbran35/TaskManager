using TaskManager.Domain.Interfaces;

namespace TaskManager.Domain.Events
{
    public class EntityDeletedEvent (Guid id): IDomainEvent
    {
        public DateTime OccurredOn { get; } = DateTime.UtcNow;
        public Guid Id { get; } = id;
    }
}