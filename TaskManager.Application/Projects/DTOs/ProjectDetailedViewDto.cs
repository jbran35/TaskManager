using TaskManager.Application.TodoItems.DTOs;
using TaskManager.Domain.Enums;
using TaskManager.Domain.Interfaces;

namespace TaskManager.Application.Projects.DTOs
{
    public record ProjectDetailedViewDto : ProjectTileDto, IProjectDetailedView
    {
        //Id, Title, Description, TotalTodoItemsCount, CompleteTodoItemsCount, Status, CreatedOn, TodoItems
        public List<TodoItemEntry> TodoItems { get; set; } = new();
        IEnumerable<ITodoItemEntry> IProjectDetailedView.TodoItems => TodoItems;
        public bool IsExpired { get; set; } = false;
        public bool IsComplete { get; set; } = false;
    }
}
