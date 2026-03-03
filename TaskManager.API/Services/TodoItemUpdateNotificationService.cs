using Microsoft.AspNetCore.SignalR;
using TaskManager.API.Hubs;
using TaskManager.Application.Interfaces;

namespace TaskManager.API.Services
{
    public class TodoItemUpdateNotificationService(IHubContext<TaskHub> hubContext) : ITodoItemUpdateNotificationService
    {
        private readonly IHubContext<TaskHub> _hubContext = hubContext;
        public async Task NotifyTodoItemUpdated(string assigneeId)
        {
            await _hubContext.Clients.User(assigneeId).SendAsync("TaskUpdated");
        }
    }
}
