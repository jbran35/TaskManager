namespace TaskManager.Application.Users.DTOs.Requests
{
    //A request DTO for registering a new user
    public record RegisterUserRequest
    {
        //UserName, Password, Email, FirstName, LastName
        public string UserName { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
    }
}
