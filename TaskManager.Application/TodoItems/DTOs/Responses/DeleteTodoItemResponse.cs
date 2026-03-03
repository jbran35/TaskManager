namespace TaskManager.Application.TodoItems.DTOs.Responses
{
    //A response DTO for deleting a Todo item
    public record DeleteTodoItemResponse
    {
        public Guid TodoItemId { get; init; }
        public string TodoItemName { get; init; } = string.Empty;
        public bool Success { get; init; }
        public string? Message { get; init; }

    }
}
