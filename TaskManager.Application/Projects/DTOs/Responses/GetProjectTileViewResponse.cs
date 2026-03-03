namespace TaskManager.Application.Projects.DTOs.Responses
{
    //A response DTO for retrieving a project's tile view information.
    public record GetProjectTileViewResponse
    {
        public ProjectTileDto? ProjectTile { get; init; }
        public bool Success { get; init; }
        public string Message { get; init; } = string.Empty;
    }
}
