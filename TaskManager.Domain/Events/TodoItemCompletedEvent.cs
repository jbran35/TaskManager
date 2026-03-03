using TaskManager.Domain.Interfaces;

namespace TaskManager.Domain.Entities
{
    public record TodoItemCompletedEvent (Guid id) : IDomainEvent
    {
        public DateTime OccurredOn { get; } = DateTime.UtcNow;
    }
}