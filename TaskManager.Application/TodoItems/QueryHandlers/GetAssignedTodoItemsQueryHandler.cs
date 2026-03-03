using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using System.Text.Json;
using TaskManager.Application.TodoItems.DTOs;
using TaskManager.Application.TodoItems.Queries;
using TaskManager.Domain.Common;
using TaskManager.Domain.Interfaces;

namespace TaskManager.Application.TodoItems.QueryHandlers
{
    public class GetAssignedTodoItemsQueryHandler(IUnitOfWork unitOfWork, IDistributedCache _cache, ILogger<GetAssignedTodoItemsQueryHandler> logger) : IRequestHandler<GetAssignedTodoItemsQuery, Result<List<TodoItemEntry>>>
    {
        private readonly IUnitOfWork _unitOfWork = unitOfWork;
        private readonly ILogger<GetAssignedTodoItemsQueryHandler> _logger = logger;

        public async Task<Result<List<TodoItemEntry>>> Handle(GetAssignedTodoItemsQuery request, CancellationToken cancellationToken)
        {
            //Validate Request
            if (request is null || request.UserId == Guid.Empty)
                return Result<List<TodoItemEntry>>.Failure("Invalid Request");

            string key = $"assigned_todo_items:{request.UserId}";

            try
            {
                _logger.LogInformation("Trying to get Assigned Tasks from Redis");
                var cachedTodoItems = await _cache.GetStringAsync(key, cancellationToken);

                if (!string.IsNullOrEmpty(cachedTodoItems))
                {
                    var tasks = JsonSerializer.Deserialize<List<TodoItemEntry>>(cachedTodoItems);
                    return Result<List<TodoItemEntry>>.Success(tasks!);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Redis Error:");
            }

            _logger.LogInformation("Getting Assigned Items From Database");

            //Get & Validate Items
            var assignedItems = await _unitOfWork.TodoItemRepository
                .GetMyAssignedTodoItemsAsync(request.UserId, cancellationToken)
                .Select(t => new TodoItemEntry
                {
                    Id = t.Id,
                    Title = t.Title,
                    ProjectTitle = t.Project.Title,
                    AssigneeName = t.Assignee != null ? t.Assignee.FullName : string.Empty,
                    OwnerName = t.Owner != null ? t.Owner.FullName : string.Empty,
                    Priority = t.Priority, 
                    DueDate = t.DueDate, 
                    CreatedOn = t.CreatedOn, 
                    Status = t.Status
                }).ToListAsync(cancellationToken);

            if (assignedItems is null)
                return Result<List<TodoItemEntry>>.Failure("Issue Retrieving Tasks");
            
            if (!assignedItems.Any())
                return Result<List<TodoItemEntry>>.Success([]);

            try
            {
                var options = new DistributedCacheEntryOptions
                {
                    SlidingExpiration = TimeSpan.FromMinutes(20),
                    AbsoluteExpirationRelativeToNow = TimeSpan.FromHours(1)

                };

                string serializedList = JsonSerializer.Serialize(assignedItems);
                await _cache.SetStringAsync(key, serializedList, options, cancellationToken);
                _logger.LogInformation("Saving Assinged Items To Redis");
            }

            catch(Exception ex)
            {
                _logger.LogError(ex, "Redis Error:");
            }

            return Result<List<TodoItemEntry>>.Success(assignedItems); 
        }
    }
}
