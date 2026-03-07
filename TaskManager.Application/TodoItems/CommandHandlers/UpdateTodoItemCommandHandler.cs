using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using TaskManager.Application.Common;
using TaskManager.Application.Interfaces;
using TaskManager.Application.TodoItems.Commands;
using TaskManager.Application.TodoItems.DTOs;
using TaskManager.Domain.Common;
using TaskManager.Domain.Entities;
using TaskManager.Domain.Interfaces;
using TaskManager.Domain.ValueObjects;

namespace TaskManager.Application.TodoItems.CommandHandlers
{
    public class UpdateTodoItemCommandHandler(IUnitOfWork unitOfWork, UserManager<User> userManager, 
        ITodoItemUpdateNotificationService updateNotificationService, IDistributedCache _cache, ILogger<UpdateTodoItemCommandHandler> logger) : IRequestHandler<UpdateTodoItemCommand, Result<TodoItemEntry>>
    {
        private readonly IUnitOfWork _unitOfWork = unitOfWork;
        private readonly UserManager<User> _userManager = userManager;
        private readonly ITodoItemUpdateNotificationService _updateNotificationService = updateNotificationService;
        private readonly ILogger<UpdateTodoItemCommandHandler> _logger = logger; 

        public async Task<Result<TodoItemEntry>> Handle(UpdateTodoItemCommand request, CancellationToken cancellationToken)
        {
            _logger.LogInformation("In Handler"); 
            var user = await _userManager.FindByIdAsync(request.UserId.ToString());
            if (user is null)
                return Result<TodoItemEntry>.Failure("User Not Found");

            var project = await _unitOfWork.ProjectRepository.GetProjectWithoutTasksAsync(request.ProjectId, cancellationToken);
            if (project is null || project.OwnerId != user.Id)
                return Result<TodoItemEntry>.Failure("Project Not Found");

            var todoItem = await _unitOfWork.TodoItemRepository.GetTodoItemByIdAsync(request.TodoItemId, cancellationToken);
            if (todoItem is null || todoItem.OwnerId != user.Id || todoItem.ProjectId != project.Id)
                return Result<TodoItemEntry>.Failure("Task Not Found");

            if (request.NewTitle is not null)
            {
                var titleResult = Title.Create(request.NewTitle);
                if (titleResult.IsFailure)
                    return Result<TodoItemEntry>.Failure(titleResult.ErrorMessage ?? "Inavlid Title");

                todoItem.UpdateTitle(titleResult.Value);
            }

            if (request.NewDescription is not null)
            {
                var descriptionResult = Description.Create(request.NewDescription);
                if (descriptionResult.IsFailure)
                    return Result<TodoItemEntry>.Failure(descriptionResult.ErrorMessage ?? "Invalid Description");

                todoItem.UpdateDescription(descriptionResult.Value);
            }

            if (request.NewPriority is not null)
                todoItem.UpdatePriority(request.NewPriority.Value);

            _logger.LogInformation("Assessing Assignee Changes");

            bool hasExistingAssignee = todoItem.AssigneeId is not null && todoItem.AssigneeId != Guid.Empty;
            string existingAssigneeId = hasExistingAssignee ? todoItem.AssigneeId!.Value.ToString() : string.Empty;
            
            bool hasRequestedAssignee = request.AssigneeId is not null && request.AssigneeId != Guid.Empty;
            bool switchingAssignees = hasExistingAssignee && hasRequestedAssignee && request.AssigneeId != todoItem.AssigneeId;
            bool unassigning = hasExistingAssignee && !hasRequestedAssignee;

            _logger.LogInformation("HasExistingAssignee: " + hasExistingAssignee + "\n ExistingAssigneeId: " + existingAssigneeId + "\n HasRequestedAssignee: " + hasRequestedAssignee
                + "\n SwitchingAssignees: " + switchingAssignees + "\n Unassigning: " + unassigning + "\n");

            //Need to send refresh notification if an assignee is involved at all.
            bool sendRefreshNotification = false;
            if (hasExistingAssignee || hasRequestedAssignee)
                sendRefreshNotification = true;

            string oldAssigneeKey = string.Empty;
            string newAssigneeKey = string.Empty; 

            if (switchingAssignees || unassigning)
            {
                oldAssigneeKey = CacheKeys.AssignedTodoItems(todoItem.AssigneeId!.Value); // switchingAssignees cannot be true if todoItem.AssigneeId is null

                if (switchingAssignees)
                {
                    var requestAssignee = await _userManager.FindByIdAsync(request.AssigneeId!.Value.ToString());
                    if (requestAssignee is null)
                        return Result<TodoItemEntry>.Failure("Assignee Not Found");

                    newAssigneeKey = CacheKeys.AssignedTodoItems(request.AssigneeId.Value); 
                    todoItem.AssignToUser(request.AssigneeId.Value);
                }

                if (unassigning)
                    todoItem.Unassign();
            }
            
            if (request.NewDueDate.HasValue && request.NewDueDate.Value != DateTime.MinValue)
                todoItem.UpdateDueDate(request.NewDueDate.Value);

            try
            {
                _unitOfWork.TodoItemRepository.Update(todoItem);
                await _unitOfWork.SaveChangesAsync(cancellationToken);
                
                var listEntryDto = new TodoItemEntry
                {
                    Id = todoItem.Id,
                    OwnerId = todoItem.OwnerId, 
                    AssigneeId = todoItem.AssigneeId,
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

                if (sendRefreshNotification)
                {
                    _logger.LogInformation("In SendRefreshNotification");
                    if (!string.IsNullOrEmpty(oldAssigneeKey))
                    {
                        _logger.LogInformation("Clearing oldAssigneeKey");
                        await _cache.RemoveAsync(oldAssigneeKey, cancellationToken);
                    }

                    if (!string.IsNullOrEmpty(newAssigneeKey))
                    {
                        _logger.LogInformation("Clearing newAssigneeKey");
                        await _cache.RemoveAsync(newAssigneeKey, cancellationToken); 
                    }
                }

                if (unassigning)
                {
                    _logger.LogInformation("Sending SignalR Refresh Notifications For Unassigning");
                    await _updateNotificationService.NotifyTodoItemUpdated(todoItem.AssigneeId!.Value.ToString());
                }

                else if (switchingAssignees)
                {
                    _logger.LogInformation("Sending SignalR Refresh Notifications For Switching Assignees"); 
                    await _updateNotificationService.NotifyTodoItemUpdated(existingAssigneeId);
                    await _updateNotificationService.NotifyTodoItemUpdated(request.AssigneeId!.Value.ToString());
                }

                else if (hasRequestedAssignee)
                {
                    _logger.LogInformation("Sending SignalR Refresh Notifications For Adding Assignee (hasRequestAssignee)");
                    await _updateNotificationService.NotifyTodoItemUpdated(request.AssigneeId!.Value.ToString());
                }

                return Result<TodoItemEntry>.Success(listEntryDto);
            }

            catch (Exception ex)
            {
                _logger.LogError(ex, "Issue Updating Task");
                return Result<TodoItemEntry>.Failure("Issue Updating Task."); 
            }
        }
    }
}
