namespace TaskManager.Application.TodoItems.DTOs.Responses
{
    // A response containing the data necessary to display the updated task in detailed view (TaskDetailsDto),
    // a success flag, and a message.
    public record UpdateTodoItemResponse
    {
        public TodoItemEntry? UpdatedTodoItem { get; init; }

    }
}

