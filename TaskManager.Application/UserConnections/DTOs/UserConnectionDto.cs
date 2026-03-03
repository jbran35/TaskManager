namespace TaskManager.Application.UserConnections.DTOs
{
    public record UserConnectionDto(
        Guid Id,
        Guid UserId,
        Guid AssigneeId,
        string AssigneeFullName,
        string AssigneeEmail
        );
}
