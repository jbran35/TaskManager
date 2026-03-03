using TaskManager.Domain.Entities;

namespace TaskManager.Domain.Interfaces
{
    public interface IProjectDetailedView : IProjectTile
    {
        public IEnumerable<ITodoItemEntry> TodoItems { get; }
    }
}
