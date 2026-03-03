namespace TaskManager.Application.UserConnections.DTOs.Responses
{
    public record AddUserToGroupResponse
    {
        public bool Success { get; set; }
        public string Message { get; set; }
    }
}
