using TaskManager.Domain.Enums;
using TaskManager.Domain.ValueObjects;
using TaskManager.Domain.Common;


namespace TaskManager.Domain.Entities
{
    public class TodoItem : Entry
    {
        public Guid ProjectId { get; private set; }
        public virtual Project Project { get; private set; }
        public Guid? AssigneeId { get; set; }
        public virtual User? Assignee { get; set; }
        public Priority? Priority { get; private set; }
        public DateTime? DueDate { get; private set; }


        private TodoItem() : base () { } // Parameterless constructor for EFCore

        //Title, Description, OwnerID, ProjectId, AssigneeId, Priority, DueDate
        private TodoItem(Title title, Description description, Guid ownerId, Guid projectId, Guid? assigneeId, 
            Priority? priority, DateTime? dueDate) : base(title, description, ownerId)
        {

            if (projectId == Guid.Empty) 
                throw new ArgumentException("Project ID cannot be empty.", nameof(projectId));

            if (priority.HasValue && !Enum.IsDefined<Priority>(priority.Value)) 
                throw new ArgumentException("Invalid priority value.", nameof(priority));

            this.AssigneeId = assigneeId;
            this.ProjectId = projectId;
            this.AssigneeId = assigneeId;
            this.Priority = priority ?? Enums.Priority.None;
            this.DueDate = dueDate;
   
        }

        public static Result<TodoItem> Create(Title title, Description description, Guid ownerId, Guid projectId, Guid? assigneeId, 
            Priority? priority, DateTime? dueDate)
        {
            if (projectId == Guid.Empty)
                return Result<TodoItem>.Failure("Project ID cannot be empty.");

            //if(assigneeId.HasValue && assigneeId.Value == Guid.Empty)
            //    return Result<TodoItem>.Failure("Assignee ID cannot be empty if provided.");

            if(ownerId == Guid.Empty)
                return Result<TodoItem>.Failure("Owner ID cannot be empty.");

            if(priority.HasValue && !Enum.IsDefined<Priority>(priority.Value))
                return Result<TodoItem>.Failure("Invalid priority value.");


            return Result<TodoItem>.Success(new TodoItem(title, description, ownerId, projectId, assigneeId, priority, dueDate));
        }

        public override Result MarkAsComplete()
        {
            var deletedCheck = CheckIfDeleted();
            if (deletedCheck.IsFailure) return deletedCheck;

            if (Project is not null && Project.Status == Status.Deleted)
                return Result.Failure("Cannot complete a todo item associated with a deleted project.");

            if (Status == Status.Complete)
                return Result.Success();

            Status = Status.Complete;
            AddDomainEvent(new TodoItemCompletedEvent(this.Id));

            return Result.Success();
        }

        public Result UpdateDueDate(DateTime? dueDate)
        {
            this.DueDate = dueDate;

            return Result.Success();
        }
        public Result UpdateProjectAssignment(Guid projectId)
        {
            if (projectId == Guid.Empty)
            {
                return Result.Failure("Project ID cannot be empty.");
            }
            this.ProjectId = projectId;

            return Result.Success();
        }

        public Result UpdatePriority(Priority priority)
        {
            if (!Enum.IsDefined<Priority>(priority))
            {
                return Result.Failure("Invalid priority value.");
            }

            this.Priority = priority;
            return Result.Success();
        }

        public Result AssignToUser(Guid userId)
        {
            if (userId == Guid.Empty)
            {
                return Result.Failure("Could not identify user to assign");
            }
            this.AssigneeId = userId;
            return Result.Success();
        }
        public Result Unassign()
        {
            this.AssigneeId = null;
            this.Assignee = null;
            return Result.Success();
        }
    }
}
