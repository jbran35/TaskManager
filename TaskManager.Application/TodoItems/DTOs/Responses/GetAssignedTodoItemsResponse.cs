namespace TaskManager.Application.TodoItems.DTOs.Responses
{
    // A response DTO for retrieving todo items assigned to the user
    public record GetAssignedTodoItemsResponse
    {
        public List<TodoItemEntry>? AssignedTasks { get; init; } = new List<TodoItemEntry>();
        public bool Success { get; init; }
        public string Message { get; init; } = string.Empty;
    }
}
