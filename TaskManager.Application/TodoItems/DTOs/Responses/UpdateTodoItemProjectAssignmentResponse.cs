namespace TaskManager.Application.TodoItems.DTOs.Responses
{
    // A reqsponse containing the result of updating a task's project assignment,
    // including the task ID, new project ID, new project title, success status, and a message.
    public record UpdateTodoItemProjectAssignmentResponse
    {
        public Guid TodoItemId { get; init; }
        public Guid NewProjectId { get; init; }
        public string NewProjectTitle { get; init; } = string.Empty;
        public bool Success { get; init; }
        public string Message { get; init; } = string.Empty;
    }
}
