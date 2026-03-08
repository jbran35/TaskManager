using MediatR;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using System.Text.Json;
using TaskManager.Application.Common;
using TaskManager.Application.Projects.DTOs;
using TaskManager.Application.TodoItems.DTOs;
using TaskManager.Application.TodoItems.Queries;
using TaskManager.Domain.Common;
using TaskManager.Domain.Interfaces;

namespace TaskManager.Application.TodoItems.QueryHandlers
{
    public class GetTodoItemDetailedViewQueryHandler(IUnitOfWork unitOfWork, ILogger<GetTodoItemDetailedViewQueryHandler> logger, 
        IDistributedCache cache) : IRequestHandler<GetTodoItemDetailedViewQuery, Result<TodoItemEntry>>
    {
        private readonly IUnitOfWork _unitOfWork = unitOfWork;
        private readonly ILogger<GetTodoItemDetailedViewQueryHandler> _logger = logger;
        private readonly IDistributedCache _cache = cache;
        public async Task<Result<TodoItemEntry>> Handle(GetTodoItemDetailedViewQuery request, CancellationToken cancellationToken)
        {

            var projectDetailsKey = CacheKeys.ProjectDetailedViews(request.UserId, request.ProjectId);
            try
            {
                _logger.LogInformation("Trying to get Project Details from Redis");
                var cachedProjectJson = await _cache.GetStringAsync(projectDetailsKey, cancellationToken);

                if (!string.IsNullOrEmpty(cachedProjectJson))
                {
                    var project = JsonSerializer.Deserialize<ProjectDetailedViewDto>(cachedProjectJson);

                    if (project is not null)
                    {
                        var cachedTodoItem = project.TodoItems.FirstOrDefault(t => t.Id == request.TodoItemId);

                        if (cachedTodoItem is not null)
                        {
                            return Result<TodoItemEntry>.Success(cachedTodoItem);

                        }
                    }

                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Redis Error:");
            }


            var todoItem = await _unitOfWork.TodoItemRepository.GetTodoItemByIdAsync(request.TodoItemId, cancellationToken);

            if (todoItem is null || todoItem.OwnerId != request.UserId || todoItem.Project.OwnerId != request.UserId)
                return Result<TodoItemEntry>.Failure("Task Not Found");

            var TodoItemDetailedViewDto = new TodoItemDetailedViewDto
            {
                Id = todoItem.Id,
                Title = todoItem.Title.Value,
                Description = todoItem.Description.Value,
                ProjectTitle = todoItem.Project.Title,
                AssigneeName = todoItem.Assignee?.FullName ?? string.Empty,
                OwnerName = todoItem.Owner?.FullName ?? string.Empty,
                Priority = todoItem.Priority ?? Domain.Enums.Priority.None,
                DueDate = todoItem.DueDate,
                CreatedOn = todoItem.CreatedOn,
                Status = todoItem.Status
            };

            return Result<TodoItemEntry>.Success(TodoItemDetailedViewDto);
        }
    }
}
