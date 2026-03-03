using TaskManager.Domain.Interfaces;

namespace TaskManager.Domain.Entities
{
    public abstract class Entity
    {
        public Guid Id { get; init; }
    

        private readonly List<IDomainEvent> _domainEvents = new List<IDomainEvent>();
        public IReadOnlyCollection<IDomainEvent> DomainEvents => _domainEvents.AsReadOnly();


        protected Entity()
        {
            Id = Guid.NewGuid();
        }

        public void AddDomainEvent(IDomainEvent domainEvent) 
        {
            _domainEvents.Add(domainEvent);
        }

        public void ClearDomainEvents()
        {
            _domainEvents.Clear();
        }

        public override bool Equals(object? obj)
        {
            if (obj is not Entity other) return false;
            if (ReferenceEquals(this, other)) return true;
            
            return Id.Equals(other.Id);
        }

        public override int GetHashCode() => Id.GetHashCode();

    }
}
