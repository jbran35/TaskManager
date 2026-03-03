using TaskManager.Domain.Enums;

namespace TaskManager.Domain.Interfaces
{
    public interface IProjectTile
    {
        //Id, Title, Description, TotalTodoItems, CompleteTodoItems, CreatedOn, NeedsUpdate
        Guid Id { get; }
        Guid OwnerId { get; }
        string Title { get; }
        string? Description { get; }
        int TotalTodoItemCount { get; }
        int CompleteTodoItemCount { get; }
        DateTime CreatedOn { get; }
        Status Status { get; }

    }
}
