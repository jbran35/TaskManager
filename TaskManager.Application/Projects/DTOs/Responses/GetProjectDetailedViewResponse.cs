namespace TaskManager.Application.Projects.DTOs.Responses
{
    //A response DTO for getting detailed view of a project (i.e., all details). 
    public record GetProjectDetailedViewResponse
    {
        public ProjectDetailedViewDto? ProjectDetails { get; init; }
    }
}
