using TaskManager.Domain.Common;
using TaskManager.Domain.Enums;
using TaskManager.Domain.Events;
using TaskManager.Domain.Interfaces;
using TaskManager.Domain.ValueObjects;

namespace TaskManager.Domain.Entities
{
    public class Project : Entry
    {

        private readonly List<TodoItem> _todoItems = new List<TodoItem>();
        public virtual IReadOnlyCollection<TodoItem> TodoItems => _todoItems.AsReadOnly();

        //public virtual int TotalTodoItemCount => _todoItems.Count;
        //public virtual int CompleteTodoItemCount => _todoItems.Count(t => t.Status == Status.Complete);

        // Parameterless constructor for EFCore
        private Project() : base() { }

        private Project(Title title, Guid ownerId, Description description)
            : base(title, description, ownerId)
        {

        }

        public static Result<Project> Create(Title title, Description description, Guid ownerId)
        {
            if (ownerId == Guid.Empty)
                return Result<Project>.Failure("OwnerId cannot be empty."); 
            
            return Result<Project>.Success(new Project(title, ownerId, description));
        }

        public override Result MarkAsComplete()
        {
            var deletedCheck = CheckIfDeleted();
            if (deletedCheck.IsFailure) return deletedCheck;

            if (Status == Status.Complete)
                return Result.Success();

            Status = Status.Complete;

            foreach (var todoItem in TodoItems)
            {
                if (todoItem.Status != Status.Deleted)
                {
                    todoItem.MarkAsComplete();
                }
            }

            AddDomainEvent(new ProjectCompletedEvent(this.Id));

            return Result.Success();
        }

        public Result AddTodoItem(TodoItem todoItem)
        {
            var deletedCheck = CheckIfDeleted();
            if (deletedCheck.IsFailure) return deletedCheck;

            if (todoItem is null)
                return Result<Project>.Failure("Need task to add to project");

            if (todoItem.Status == Status.Deleted)
                return Result<Project>.Failure("Cannot add a deleted TodoItem to a project.");

            if (todoItem.OwnerId != this.OwnerId)
                return Result<Project>.Failure("TodoItem owner does not match project owner.");

            if (todoItem.ProjectId != this.Id)
                return Result<Project>.Failure("TodoItem does not belong to this project.");


            if (TodoItems.Any(t => t.Id == todoItem.Id))
                return Result<Project>.Failure("TodoItem is already added to this project.");

            _todoItems.Add(todoItem);
            AddDomainEvent(new TodoItemAddedEvent(this.Id));

            return Result.Success();
        }

        public Result DeleteTodoItem(TodoItem todoItem)
        {
            var deletedCheck = CheckIfDeleted();
            if (deletedCheck.IsFailure) return deletedCheck;

            if (todoItem is null)
                return Result<Project>.Failure("Error deleting task");

            if (todoItem.ProjectId != this.Id)
                return Result<Project>.Failure("TodoItem does not belong to this project.");

            if (!_todoItems.Any())
                return Result<Project>.Failure("No TodoItems are associated with this project.");

            if (!_todoItems.Contains(todoItem))
                return Result<Project>.Failure("TodoItem is not part of this project.");

            if (todoItem.Status == Status.Deleted)
                return Result<Project>.Failure("TodoItem is already deleted.");

            todoItem.MarkAsDeleted(); 
            _todoItems.Remove(todoItem);
            AddDomainEvent(new TodoItemRemovedEvent(this.Id));

            return Result.Success();
        }
    }
}
