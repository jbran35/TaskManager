using System.ComponentModel.DataAnnotations;

namespace TaskManager.Application.Users.DTOs.Requests
{
    //Request DTO for user login
    public record LoginUserRequest
    {
        [Required(ErrorMessage = "Username is required")]
        public required string UserName { get; set; } = string.Empty;
        [Required(ErrorMessage = "Password is required")]
        public required string Password { get; set; } = string.Empty;
    }
}
