using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaskManager.Domain.Interfaces;

namespace TaskManager.Domain.Events
{
    public record TodoItemRemovedEvent(Guid id) : IDomainEvent
    {
        public DateTime OccurredOn { get; } = DateTime.UtcNow;

    }
}
