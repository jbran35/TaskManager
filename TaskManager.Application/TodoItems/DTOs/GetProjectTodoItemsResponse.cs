namespace TaskManager.Application.TodoItems.DTOs
{
    //A DTO for returning a list of todo items associated with a specific project,
    //with only the data needed to display them in a table. 
    public record GetProjectTodoItemsResponse
    {
        public IEnumerable<TodoItemEntry>? TodoItems { get; set; }
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;

    }
}
