using System.ComponentModel.DataAnnotations;

namespace TaskManager.Presentation.Components.Models
{
    public class ValidationModel
    {
        //Title, Description, AssigneeId, Priority, DueDate
        [Required(ErrorMessage = "Task name is required.")]
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public Guid? AssigneeId { get; set; }
        public Domain.Enums.Priority Priority { get; set; } = Domain.Enums.Priority.None;
        public DateTime? DueDate { get; set; }
    }
}
