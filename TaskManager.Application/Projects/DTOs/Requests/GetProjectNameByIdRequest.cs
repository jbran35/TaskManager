using System.ComponentModel.DataAnnotations;

namespace TaskManager.Application.DTOs.Requests

{
    // DTO for requesting a project name by its ID
    public record GetProjectNameByIdRequest
    {
        [Required(ErrorMessage= "ProjectId is required")]
        public required Guid ProjectId { get; set; } = Guid.Empty;

        [Required(ErrorMessage = "Id is required")]
        public required Guid UserId { get; set; } = Guid.Empty;
    };
}
