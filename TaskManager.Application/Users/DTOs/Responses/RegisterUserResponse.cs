namespace TaskManager.Application.Users.DTOs.Responses
{
    //Response DTO for user registration
    public record RegisterUserResponse
    {
        public bool Success { get; init; } = false;
        public string Message { get; init; } = string.Empty;
    }
}
