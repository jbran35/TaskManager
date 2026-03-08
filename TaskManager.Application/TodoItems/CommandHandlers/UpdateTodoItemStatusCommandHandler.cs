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
using TaskManager.Domain.Enums;
using TaskManager.Domain.Interfaces;


namespace TaskManager.Application.TodoItems.CommandHandlers
{
    public class UpdateTodoItemStatusCommandHandler(IUnitOfWork unitOfWork, UserManager<User> userManager, ITodoItemUpdateNotificationService updateService,
        IDistributedCache cache, ILogger<UpdateTodoItemStatusCommandHandler> logger) : IRequestHandler<UpdateTodoItemStatusCommand, Result<TodoItemEntry>>
    {
        private readonly IUnitOfWork _unitOfWork = unitOfWork;
        private readonly UserManager<User> _userManager = userManager;
        private readonly ITodoItemUpdateNotificationService _updateService = updateService;
        private readonly IDistributedCache _cache = cache;
        private readonly ILogger<UpdateTodoItemStatusCommandHandler> _logger = logger;
        public async Task<Result<TodoItemEntry>> Handle(UpdateTodoItemStatusCommand request, CancellationToken cancellationToken)
        {
            _logger.LogInformation("In Handler"); 
            var user = await _userManager.FindByIdAsync(request.UserId.ToString());
            if(user is null)
                return Result<TodoItemEntry>.Failure("User Not Found");

            var todoItem = await _unitOfWork.TodoItemRepository.GetTodoItemByIdAsync(request.TodoItemId, cancellationToken);
            if(todoItem is null)
                return Result<TodoItemEntry>.Failure("TodoItem Not Found");

            bool hasAssignee = todoItem.AssigneeId is not null && todoItem.AssigneeId != Guid.Empty;
            bool userIsOwner = user.Id == todoItem.OwnerId;
            bool userIsAssignee = user.Id == todoItem.AssigneeId;

            if (todoItem is null || todoItem.Project is null || (!userIsAssignee && !userIsOwner))
                return Result<TodoItemEntry>.Failure("You Do Not Have Access To This Project Or Task");

            if(todoItem.Project.Status == Status.Deleted)
                return Result<TodoItemEntry>.Failure("This Project Has Been Deleted");


            if (todoItem.Status == Status.Incomplete)
                todoItem.MarkAsComplete();

            else if (todoItem.Status == Status.Complete)
            {
                todoItem.MarkAsIncomplete();
                todoItem.Project.MarkAsIncomplete();
            }

            try
            {
                _logger.LogInformation("Trying to update/Save");
                _unitOfWork.TodoItemRepository.Update(todoItem);
                await _unitOfWork.SaveChangesAsync(cancellationToken);

                var listEntryDto = new TodoItemEntry

                {
                    Id = todoItem.Id,
                    Title = todoItem.Title,
                    ProjectTitle = todoItem.Project.Title,
                    AssigneeName = todoItem.Assignee?.FullName,
                    OwnerName = todoItem.Owner?.FullName ?? string.Empty,
                    Priority = todoItem.Priority ?? Priority.None,
                    DueDate = todoItem.DueDate,
                    CreatedOn = todoItem.CreatedOn,
                    Status = todoItem.Status
                };

                //If assignee is updating > We need to notify the owner
                if (hasAssignee && userIsAssignee)
                {
                    Console.WriteLine("Notifying Owner");
                    var ownerDetailedViewKey = CacheKeys.ProjectDetailedViews(todoItem.OwnerId, todoItem.ProjectId);
                    var assignedItemsKey = CacheKeys.AssignedTodoItems(user.Id);
                    await _cache.RemoveAsync(assignedItemsKey, CancellationToken.None);
                    await _cache.RemoveAsync(ownerDetailedViewKey, CancellationToken.None);
                    await _updateService.NotifyTodoItemUpdated(todoItem.OwnerId.ToString());
                }

                //If owner is updating > We need to notify assignee
                if (hasAssignee && userIsOwner)
                {
                    var detailsKey = CacheKeys.ProjectDetailedViews(user.Id, todoItem.ProjectId);
                    var assignedItemsKey = CacheKeys.AssignedTodoItems(todoItem.AssigneeId!.Value);
                    
                    _logger.LogInformation("Clearing owner cache keys");
                    await _cache.RemoveAsync(detailsKey, CancellationToken.None);
                    await _cache.RemoveAsync(assignedItemsKey, CancellationToken.None); 
                    await _updateService.NotifyTodoItemUpdated(todoItem.AssigneeId!.Value.ToString());
                }


                return Result<TodoItemEntry>.Success(listEntryDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Issue updating task status");
                return Result<TodoItemEntry>.Failure("Error Marking Task As Complete.");
            }
        }
    }
}
