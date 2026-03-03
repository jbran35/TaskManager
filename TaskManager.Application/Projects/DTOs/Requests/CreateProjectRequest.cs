using System.ComponentModel.DataAnnotations;

namespace TaskManager.Application.DTOs.Requests
{
    /// Request DTO for creating a new Project.
    public record CreateProjectRequest
    {
        [Required(ErrorMessage = "Cannot Create Project Without a Name.")]
        public required string Title { get; set; }
        public string? Description { get; set; }
    }
}
