using TaskManager.Domain.Enums;
using TaskManager.Domain.Interfaces;

namespace TaskManager.Application.TodoItems.DTOs
{
    public record TodoItemEntry : ITodoItemEntry
    {
        //Id, AssigneeId, OwnerId, Title, Description, ProjectTitle, AssigneeName, OwnerName, Priority, DueDate, CreatedOn, Status
        public Guid Id { get; set; }
        public Guid? AssigneeId { get; set; }
        public Guid OwnerId { get; set; }
        public string Title { get; set; } = string.Empty;
        public string? Description { get; set; } = string.Empty;
        public string ProjectTitle { get; set; } = string.Empty;
        public string? AssigneeName { get; set; }
        public string OwnerName { get; set; } = string.Empty;
        public Priority? Priority { get; set; } = Domain.Enums.Priority.None;
        public DateTime? DueDate { get; set; }
        public DateTime CreatedOn { get; set; }
        public Status Status { get; set; }
    }
}
