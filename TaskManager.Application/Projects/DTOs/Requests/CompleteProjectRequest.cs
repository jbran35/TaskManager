namespace TaskManager.Application.Projects.DTOs.Requests
{
    public record CompleteProjectRequest
    {
        Guid UserId { get; set; }
        Guid ProjectId { get; set; }
        Guid? AssigneeId { get; set; } = null; 
    }
}
