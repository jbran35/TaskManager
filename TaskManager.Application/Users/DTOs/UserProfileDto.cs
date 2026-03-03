namespace TaskManager.Application.Users.DTOs
{
    public record UserProfileDto(
        Guid Id,
        string FirstName,
        string LastName,
        string Email,
        string UserName); 

}