using System.Text.Json.Serialization;

namespace TaskManager.Application.Users.DTOs.Responses
{
    //A response DTO for user login
    public record LoginUserResponse
    {
        public Guid Id { get; set; } = Guid.Empty;
        public string UserName { get; set; } = string.Empty;
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;

        public string Email { get; set; } = string.Empty; 
        public string? Token { get; set; } = string.Empty;
        public bool Success { get; set; } = false;
        public string? Message { get; set; } = string.Empty;
    }
}
