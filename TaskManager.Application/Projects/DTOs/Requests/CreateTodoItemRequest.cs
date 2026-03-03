using System.ComponentModel.DataAnnotations;
using TaskManager.Domain.Enums;

namespace TaskManager.Application.Projects.DTOs.Requests

{
    // Request DTO for creating a new todo item
    // Taking in: Name, ProjectId, ProjectName, Description, AssigneeId, AssigneeFirstName, AssigneeLastName, Status, Priority, DueDate


    public record CreateTodoItemRequest
    {
        [Required(ErrorMessage = "Cannot Create a New Task Without a Name")]
        public required string Title { get; set; }

        [Required(ErrorMessage = "Need to Assign New Task To Project")]
        public required Guid ProjectId { get; set; }
        public string? Description { get; set; }
        public Guid? AssigneeId { get; set; } = Guid.Empty;
        public Status? Status { get; set; }
        public Priority? Priority { get; set; }
        public DateTime? DueDate { get; set; }


    }
}
