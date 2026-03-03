using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Caching.Distributed;
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
            // Validate request
            if (request is null || request.UserId == Guid.Empty || request.ProjectId == Guid.Empty || request.TodoItemId == Guid.Empty)
                return Result<TodoItemEntry>.Failure("Invalid Request.");

            // Check if user exists
            var user = await _userManager.FindByIdAsync(request.UserId.ToString());

            if (user is null)
                return Result<TodoItemEntry>.Failure("User Not Found");

            var project = await _unitOfWork.ProjectRepository.GetProjectWithoutTasksAsync(request.ProjectId, cancellationToken);

            // Check if project exists and belongs to the user
            if (project is null || project.OwnerId != user.Id)
                return Result<TodoItemEntry>.Failure("Project Not Found");

            // Check if todo item exists and belongs to the user and project
            var todoItem = await _unitOfWork.TodoItemRepository.GetTodoItemByIdAsync(request.TodoItemId, cancellationToken);

            if (todoItem is null || todoItem.OwnerId != user.Id || todoItem.ProjectId != project.Id)
                return Result<TodoItemEntry>.Failure("Task Not Found");

            //Check if assignee exists - if so, clear their assignee's cached assigned items

            string key = $"assigned_tasks_";
            bool sendRefreshNotification = false;

            if (todoItem.AssigneeId != Guid.Empty && todoItem.AssigneeId is not null)
            {
               
                var idString = todoItem.AssigneeId.ToString() ?? string.Empty;

                var assignee = await _userManager.FindByIdAsync(idString);

                if (assignee is not null)
                {
                    Console.WriteLine("\n \n SETTING SENDREFRESHNOTIFICATION TO TRUE \n \n");
                    key = key + assignee.Id;
                    sendRefreshNotification = true;
                }
            }


            //Update properties if they are provided in the request
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

            if (request.AssigneeId  is not null && request.AssigneeId != Guid.Empty)
            {
                var assignee = await _userManager.FindByIdAsync(request.AssigneeId.Value.ToString());
                
                if (assignee is null)
                    return Result<TodoItemEntry>.Failure("Assignee Not Found");
                
                todoItem.AssignToUser(assignee.Id);
            }

            else if (request.AssigneeId == Guid.Empty)
            {
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

                if (todoItem.AssigneeId.ToString() is not null)
                {
                    await _updateNotificationService.NotifyTodoItemUpdated(todoItem.AssigneeId.ToString());
                }

                if (sendRefreshNotification)
                {
                    Console.WriteLine("\n \n CLEARING ASSIGNEES CACHED ITEMS \n \n");
                    _cache.Remove(key); ;
                }



                return Result<TodoItemEntry>.Success(listEntryDto);
            }

            catch (Exception)
            {
                return Result<TodoItemEntry>.Failure("Issue Updating Task."); 
            }

        }
    }
}
