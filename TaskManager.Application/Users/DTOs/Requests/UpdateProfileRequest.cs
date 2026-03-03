namespace TaskManager.Application.Users.DTOs.Requests
{
    public record UpdateProfileRequest
    {
        public Guid Id { get; set; }
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public string? Email { get; set; }
        public string? UserName { get; set; }







    }
}
