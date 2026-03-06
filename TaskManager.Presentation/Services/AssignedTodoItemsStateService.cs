using Microsoft.AspNetCore.SignalR;
using TaskManager.Application.TodoItems.DTOs;

namespace TaskManager.Presentation.Services
{
    public class AssignedTodoItemsStateService()
    {
        private List<TodoItemEntry> _assignedTodoItemsCache = new List<TodoItemEntry>();

        public List<TodoItemEntry>? GetTodoItems()
        {
            return _assignedTodoItemsCache;
        }

        public void SetTodoItemsAsync(List<TodoItemEntry> todoItems)
        {
            _assignedTodoItemsCache = todoItems;
        }

        public void AddTodoItem(TodoItemEntry newItem)
        {
            _assignedTodoItemsCache.Add(newItem);
        }

        public void Clear()
        {
            _assignedTodoItemsCache.Clear();
        }
    }
}
