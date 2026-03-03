using System.ComponentModel.DataAnnotations;

namespace TaskManager.API.DTOs.Account
{
    public class RegisterModel
    {
        [Required(ErrorMessage = "A Username is required in order to create an account")]
        public string? Username { get; set; }

        [EmailAddress]
        [Required(ErrorMessage = "An email is required in order to create an account")]
        public string? Email { get; set; }

        [Required(ErrorMessage = "A password is required in order to create an account")]
        public string? Password { get; set; }

        [Required(ErrorMessage = "A first name is required in order to create an account")]
        public string? FirstName { get; set; }
        [Required(ErrorMessage = "A last name is required in order to create an account")]
        public string? LastName { get; set; }

    }
}
