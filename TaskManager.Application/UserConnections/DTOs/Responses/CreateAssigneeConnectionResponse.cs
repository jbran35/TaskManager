namespace TaskManager.Application.UserConnections.DTOs.Responses
{
    //A response DTO for creating a new user connection
    public record CreateAssigneeConnectionResponse
    {
        public UserConnectionDto? Assignee { get; set; }
        public bool? AlreadyConnected { get; set; } = false;
        public bool isParentAddModal { get; set; } = false; 
        public bool Success { get; set; }
        public string Message { get; set; }
    }
}
