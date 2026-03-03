using System.ComponentModel.DataAnnotations;

namespace TaskManager.Domain.Entities
{
    //Class that represents an assigner/assignee relationship between two users,
    //used to preload previously assigned users
    public class UserConnection : Entity
    {
        public Guid UserId { get; set; }
        public User? User { get; private set; }

        public Guid AssigneeId { get; set; }
        public User? Assignee { get; private set; }

        private UserConnection() : base() { }

        private UserConnection(Guid userId, Guid assigneeId)
            : base()
        {
            if (userId == Guid.Empty)
                throw new ArgumentException("User ID cannot be empty.", nameof(userId));

            if (assigneeId == Guid.Empty)
                throw new ArgumentException("Assignee ID cannot be empty.", nameof(assigneeId));

            if (userId == assigneeId)
                throw new ArgumentException("User ID and Assignee ID cannot be the same.");

            UserId = userId;
            AssigneeId = assigneeId;
        }

        public static UserConnection Create(Guid userId, Guid assigneeId)
        {
            return new UserConnection(userId, assigneeId);
        }
    }
}
