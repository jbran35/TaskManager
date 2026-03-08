namespace TaskManager.Application.UserConnections.DTOs
{
    public record UserConnectionDto
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public Guid AssigneeId { get; set; }
        public string AssigneeName { get; set; } = string.Empty;
        public string AssigneeEmail { get; set; } = string.Empty;
    }
     
}
