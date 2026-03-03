namespace TaskManager.Application.Projects.DTOs.Responses
{
    //A response DTO for updating project details.
    public record UpdateProjectResponse
    {
        public ProjectDetailsDto? ProjectDetails { get; init; }
    }
}
