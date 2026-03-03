using System.ComponentModel.DataAnnotations;

namespace TaskManager.Application.DTOs.Requests
{
    //A request DTO for updating a project's details.
    public record UpdateProjectRequest
    {
        [Required(ErrorMessage = "Project Name is required.")]
        public required string Title { get; init; }
        public string? Description { get; init; }

    }
}
