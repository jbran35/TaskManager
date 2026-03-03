using System.ComponentModel.DataAnnotations;
using TaskManager.Domain.Enums;

namespace TaskManager.Application.TodoItems.DTOs.Requests
{
    /// A request to update the details of a task, including the task's ID, 
    /// user ID, title, description, assignee ID, status, priority, and due date.

    /// Id, Id, ProjectID, Name?, Description, AssigneeId, Status, Priority, DueDate
    public record UpdateTodoItemRequest
    {
        [Required(ErrorMessage = "Project ID is required.")]
        public required Guid ProjectId { get; init; }
        public string? Title { get; init; }
        public string? Description { get; init; }
        public Guid? AssigneeId { get; init; }
        public Status? Status { get; init; }
        public Priority? Priority { get; init; }
        public DateTime? DueDate { get; init; }
    }
}
