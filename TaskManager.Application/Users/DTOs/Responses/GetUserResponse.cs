namespace TaskManager.Application.Users.DTOs.Responses
{
    //A response DTO for retrieving user information
    public record GetUserResponse
    {
        //Id, FirstName, LastName, Email
        public Guid Id { get; init; }
        public string FirstName { get; init; } = string.Empty;
        public string LastName { get; init; } = string.Empty;
        public string Email { get; init; } = string.Empty;
    }
}
