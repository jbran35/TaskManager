using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using TaskManager.Application.Common;
using TaskManager.Application.Projects.Commands;
using TaskManager.Application.TodoItems.DTOs;
using TaskManager.Domain.Common;
using TaskManager.Domain.Entities;
using TaskManager.Domain.Interfaces;
using TaskManager.Domain.ValueObjects;

namespace TaskManager.Application.Projects.CommandHandlers
{
    public class AddTodoItemCommandHandler(IUnitOfWork unitOfWork, UserManager<User> userManager, IDistributedCache cache,
        ILogger<AddTodoItemCommandHandler> logger) : IRequestHandler<AddTodoItemCommand, Result<TodoItemEntry>>
    {
        private readonly IUnitOfWork _unitOfWork = unitOfWork;
        private readonly UserManager<User> _userManager = userManager;
        private readonly IDistributedCache _cache = cache;
        private readonly ILogger<AddTodoItemCommandHandler> _logger = logger;
        public async Task<Result<TodoItemEntry>> Handle(AddTodoItemCommand request, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Handling Command"); 

            //Check if the request is valid
            if (request is null || request.UserId == Guid.Empty)
                return Result<TodoItemEntry>.Failure("Invalid request.");

            if(request.ProjectId == Guid.Empty)
                return Result<TodoItemEntry>.Failure("Invalid project ID.");

            //Check if the user exists
            var user = await _userManager.FindByIdAsync(request.UserId.ToString());
            
            if (user is null)
                return Result<TodoItemEntry>.Failure("User not found.");

            //Check if the project exists and belongs to the user
            var project = await _unitOfWork.ProjectRepository.GetProjectWithoutTasksAsync(request.ProjectId, cancellationToken);

            if (project is null)
                return Result<TodoItemEntry>.Failure("Project not found.");

            if(!project.OwnerId.Equals(request.UserId))
                return Result<TodoItemEntry>.Failure("Unauthorized.");

            string? assigneeId = null; 

            //If the assigneeId is an Empty Guid, so it is clear it is unassigned and doesn't violate Foreign Key rule
            if(request.AssigneeId != Guid.Empty && request.AssigneeId is not null)
                assigneeId = request.AssigneeId.ToString();

            //assigneeId = request.AssigneeId == Guid.Empty ? null : request.AssigneeId;
            User? assignee = null;

            if (!string.IsNullOrWhiteSpace(assigneeId))
            {
                assignee = await _userManager.FindByIdAsync(assigneeId.ToString());

                if (assignee is null)
                    return Result<TodoItemEntry>.Failure("Assignee Could Not Be Found.");
            }

            var todoItemTitleResult = Title.Create(request.Title);

            if (todoItemTitleResult.IsFailure)
                return Result<TodoItemEntry>.Failure(todoItemTitleResult.ErrorMessage ?? "Invalid project title.");

            var todoItemDescriptionResult = Description.Create(request.Description!);

            if (todoItemDescriptionResult.IsFailure)
                return Result<TodoItemEntry>.Failure(todoItemDescriptionResult.ErrorMessage ?? "Invalid project description.");

            //Validate the todo item details
            var todoItemResult = TodoItem.Create(todoItemTitleResult.Value, todoItemDescriptionResult.Value, request.UserId, request.ProjectId, request.AssigneeId,
                request.Priority, request.DueDate);

            if (todoItemResult.IsFailure)
                return Result<TodoItemEntry>.Failure(todoItemResult.ErrorMessage ?? "Failed to create todo item.");

            var todoItem = todoItemResult.Value;

            project.AddTodoItem(todoItem);
            project.MarkAsIncomplete();
            _unitOfWork.TodoItemRepository.Add(todoItem);

            try
            {
                _logger.LogInformation("Saving Changes");

                await _unitOfWork.SaveChangesAsync(cancellationToken);

                var listEntryDto = new TodoItemEntry
                {
                    Id = todoItem.Id,
                    AssigneeId = todoItem.AssigneeId, 
                    OwnerId = todoItem.OwnerId,
                    Title = todoItem.Title,
                    Description = todoItem.Description,
                    ProjectTitle = todoItem.Project.Title,
                    AssigneeName = todoItem.Assignee?.FullName ?? string.Empty,
                    OwnerName = todoItem.Owner?.FullName ?? string.Empty,
                    Priority = todoItem.Priority ?? Domain.Enums.Priority.None,
                    DueDate = todoItem.DueDate,
                    CreatedOn = todoItem.CreatedOn,
                    Status = todoItem.Status
                };

                _logger.LogInformation("Clearing Project Details From Redis");

                string detailsKey = CacheKeys.ProjectDetailedViews(user.Id, project.Id);
                string tilesKey = CacheKeys.ProjectTiles(user.Id); 

                await _cache.RemoveAsync(detailsKey, cancellationToken);
                await _cache.RemoveAsync(tilesKey, cancellationToken); 

                if (listEntryDto is not null && listEntryDto.AssigneeId != Guid.Empty && listEntryDto.AssigneeId is not null)
                {
                    string assigneeKey = CacheKeys.AssignedTodoItems(listEntryDto.AssigneeId ?? Guid.Empty);
                    await _cache.RemoveAsync(assigneeKey, cancellationToken);
                }


                return Result<TodoItemEntry>.Success(listEntryDto);
            }

            catch (Exception ex)
            {
                _logger.LogError(ex, "Issue Encountered"); 
                return Result<TodoItemEntry>.Failure($"An error occurred while adding the todo item.");
            }
        }
    }
}
