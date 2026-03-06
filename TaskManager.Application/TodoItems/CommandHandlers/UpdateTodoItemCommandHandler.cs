using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Caching.Distributed;
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
        ITodoItemUpdateNotificationService updateNotificationService, IDistributedCache _cache) : IRequestHandler<UpdateTodoItemCommand, Result<TodoItemEntry>>
    {
        private readonly IUnitOfWork _unitOfWork = unitOfWork;
        private readonly UserManager<User> _userManager = userManager;
        private readonly ITodoItemUpdateNotificationService _updateNotificationService = updateNotificationService;

        public async Task<Result<TodoItemEntry>> Handle(UpdateTodoItemCommand request, CancellationToken cancellationToken)
        {
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

            bool hasExistingAssignee = todoItem.AssigneeId is not null && todoItem.AssigneeId != Guid.Empty;
            string existingAssigneeId = hasExistingAssignee ? todoItem.AssigneeId!.Value.ToString() : string.Empty;
            
            bool hasRequestedAssignee = request.AssigneeId is not null && request.AssigneeId != Guid.Empty;
            bool switchingAssignees = hasExistingAssignee && hasRequestedAssignee && request.AssigneeId != todoItem.AssigneeId;
            bool unassigning = hasExistingAssignee && !hasRequestedAssignee; 

            //Need to send refresh notification if an assignee is involved at all.
            bool sendRefreshNotification = false;
            if (hasExistingAssignee || hasRequestedAssignee)
                sendRefreshNotification = true;

            //If switching assignees - must remove old assignee's redis key here
            string currAssigneeKey = string.Empty;

            if (switchingAssignees || unassigning)
            {
                currAssigneeKey = CacheKeys.AssignedTodoItems(todoItem.AssigneeId!.Value); // switchingAssignees cannot be true if todoItem.AssigneeId is null

                if (switchingAssignees)
                {
                    var requestAssignee = await _userManager.FindByIdAsync(request.AssigneeId!.Value.ToString());
                    if (requestAssignee is null)
                        return Result<TodoItemEntry>.Failure("Assignee Not Found");

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
                    await _cache.RemoveAsync(currAssigneeKey, cancellationToken);

                if (unassigning)
                    await _updateNotificationService.NotifyTodoItemUpdated(todoItem.AssigneeId!.Value.ToString());

                else if (switchingAssignees)
                {
                    await _updateNotificationService.NotifyTodoItemUpdated(existingAssigneeId);
                    await _updateNotificationService.NotifyTodoItemUpdated(request.AssigneeId!.Value.ToString());
                }

                else if (hasRequestedAssignee)
                    await _updateNotificationService.NotifyTodoItemUpdated(request.AssigneeId!.Value.ToString());

                return Result<TodoItemEntry>.Success(listEntryDto);
            }

            catch (Exception)
            {
                return Result<TodoItemEntry>.Failure("Issue Updating Task."); 
            }
        }
    }
}
