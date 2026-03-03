using System.ComponentModel.DataAnnotations;
using TaskManager.Domain.Enums;
using TaskManager.Presentation.Components.Pages;

namespace TaskManager.Presentation
{
    public record TodoItemModel
    {

        //Title, ProjectId, ProjectName, Description, AssigneeId, Status, Priority, DueDate
        public string? Title { get; set; }
        public Guid? ProjectId { get; set; }
        public string? Description { get; set; }
        public Guid? AssigneeId { get; set; }
        public Priority? Priority { get; set; }
        public DateTime? DueDate { get; set; }

        //public NewTask? _addTaskForm; 
    }
}
