namespace TaskManager.Application.UserConnections.DTOs.Responses
{
    //A response DTO for retrieving the list of assignees a user has assigned tasks to in the past.
    public record GetAssigneesResponse
    {
        public List<UserConnectionDto> Assignees { get; init; } = new List<UserConnectionDto>();

        public bool? Success { get; init; }
        public string? Message { get; init; } = string.Empty;

    }
}
