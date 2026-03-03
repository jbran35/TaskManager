using TaskManager.Domain.Enums;

namespace TaskManager.Domain.Interfaces
{
    public interface ITodoItemEntry
    {
        //Id, Title, ProjectTitle, AssigneeName, OwnerName, Priority, DueDate, CreatedOn, Status
        public Guid Id { get; }
        public Guid OwnerId { get; }
        public Guid? AssigneeId { get; }
        public string Title { get; }
        public string? Description { get; }
        public string ProjectTitle { get; }
        public string? AssigneeName { get; }
        public string OwnerName { get; }
        public Priority? Priority { get; }
        public DateTime? DueDate { get; }
        public DateTime CreatedOn { get; }
        public Status Status { get; }
    }
}
