namespace TaskManager.Application.TodoItems.DTOs.Responses
{
    //A DTO for returning detailed view of a TodoItem
    public record GetTodoItemDetailedViewResponse
    {
        public TodoItemEntry? TodoItemDetails { get; init; }
        public bool Success { get; init; }
        public string Message { get; init; } = string.Empty;

    }
}
