using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TaskManager.Application.Users.DTOs.Responses
{
    public record UpdateProfileResponse
    {
        public Guid Id { get; set; }
        public string NewFirstName { get; set; } = string.Empty;
        public string NewLastName { get; set; } = string.Empty;
        public string NewEmail { get; set; } = string.Empty;
        public string NewUserName { get; set; } = string.Empty;
        public bool Success { get; set; } = false;
        public string Message { get; set; } = string.Empty; 

    }
}
