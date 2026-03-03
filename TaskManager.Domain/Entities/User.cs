using Microsoft.AspNetCore.Identity;


namespace TaskManager.Domain.Entities
{
    public class User : IdentityUser<Guid>
    {
        public required string FirstName { get; set; }
        public required string LastName { get; set; }
        public string FullName => $"{FirstName} {LastName}";

        public ICollection<UserConnection> Connections { get; set; } = [];
        public ICollection<UserConnection> ConnectedTo { get; set; } = [];
    }
}
