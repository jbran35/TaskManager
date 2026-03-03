using Microsoft.AspNetCore.SignalR;
using TaskManager.Application.TodoItems.DTOs;

namespace TaskManager.Presentation.Services
{
    public class AssignedTodoItemsStateService()
    {


        //private readonly IHttpClientFactory _httpClientFactory;
        //private readonly TokenProviderService _tokenProvider;

        //public AssignedTodoItemsStateService(IHttpClientFactory httpClientFactory, TokenProviderService tokenProvider)
        //{
        //    _httpClientFactory = httpClientFactory;
        //    _tokenProvider = tokenProvider;
        //}

        //public async Task<List<TodoItemEntry>> LoadMyTasksAsync()
        //{
        //    var client = _httpClientFactory.CreateClient("API");

        //    // Pull the ID out of the private pocket
        //    if (!string.IsNullOrEmpty(_tokenProvider.Token))
        //    {
        //        client.DefaultRequestHeaders.Authorization =
        //            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", _tokenProvider.Token);
        //    }

        //    // Now the API knows exactly who you are, even on a background thread!
        //    var response = await client.GetAsync("api/todoitems/MyAssignedTasks");

        //    var result = await response.Content.ReadFromJsonAsync<List<TodoItemEntry>>();

        //    return result;
        //    // ... return data
        //}
        
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

        //public void RemoveTodoItem(Guid id)
        //{
        //    _assignedTodoItemsCache.Remove(item);
        //}

        public async Task UpdateTodoItemAsync(Guid id)
        {

        }


        //public void ClearAssigneeFromTodoItems(Guid id)
        //{
        //    foreach (var todoItem in _projectCache)
        //    {
        //        var tasksToUpdate = todoItem.Value.Where(t => t.AssigneeId == id).ToList();

        //        foreach (var task in tasksToUpdate)
        //        {
        //            var updatedTask = new TodoItemListEntryDto
        //            {
        //                //Includes: ProjectId, ProjectName, TodoItemId, Title, AssigneeId, AssigneeName, DueDate, Status, Priority

        //                ProjectId = task.ProjectId,
        //                ProjectName = task.ProjectName,
        //                TodoItemId = task.TodoItemId,
        //                Title = task.Title,
        //                AssigneeId = Guid.Empty,
        //                AssigneeName = string.Empty,
        //                DueDate = task.DueDate,
        //                Status = task.Status,
        //                Priority = task.Priority
        //            };

        //            MarkExpiredAsync(task.ProjectId);



        //        }

        //    }
        //}

        public void Clear()
        {
            _assignedTodoItemsCache.Clear();
        }
    }
}
