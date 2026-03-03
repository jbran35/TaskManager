namespace TaskManager.Application.Interfaces
{
    public interface ITodoItemUpdateNotificationService
    {
        Task NotifyTodoItemUpdated(string id);
    }
}
