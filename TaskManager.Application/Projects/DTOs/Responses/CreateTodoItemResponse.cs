using TaskManager.Application.TodoItems.DTOs;

namespace TaskManager.Application.Projects.DTOs.Responses
{
    // Response DTO for creating a new todo item
    public record CreateTodoItemResponse
    {
        public TodoItemEntry? TodoItemListEntry { get; init; }
        public bool Success { get; init; }
        public string Message { get; init; } = string.Empty;
    }
}
