namespace TaskManager.Application.TodoItems.DTOs.Responses
{
    //A record to represent the response after requesting to completing a todo item

    public record UpdateTodoItemStatusResponse(TodoItemEntry ListEntry);
   
}
