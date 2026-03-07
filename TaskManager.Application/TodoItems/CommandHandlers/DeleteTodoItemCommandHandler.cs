using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using TaskManager.Application.Common;
using TaskManager.Application.TodoItems.Commands;
using TaskManager.Application.TodoItems.Events;
using TaskManager.Domain.Common;
using TaskManager.Domain.Entities;
using TaskManager.Domain.Interfaces;

namespace TaskManager.Application.TodoItems.CommandHandlers
{
    public class DeleteTodoItemCommandHandler(IUnitOfWork unitOfWork, UserManager<User> userManager, IDistributedCache cache, 
        ILogger<DeleteTodoItemCommandHandler> logger, IMediator mediator) : IRequestHandler<DeleteTodoItemCommand, Result>
    {
        private readonly IUnitOfWork _unitOfWork = unitOfWork;
        private readonly UserManager<User> _userManager = userManager;
        private readonly IDistributedCache _cache = cache;
        private readonly ILogger<DeleteTodoItemCommandHandler> _logger = logger;
        private readonly IMediator _mediator = mediator; 
        public async Task<Result> Handle(DeleteTodoItemCommand request, CancellationToken cancellationToken)
        {
            var user = await _userManager.FindByIdAsync(request.UserId.ToString());
            if(user is null)
                return Result.Failure("User Not Found");
           
            var todoItem = await _unitOfWork.TodoItemRepository.GetTodoItemByIdAsync(request.TodoItemId, cancellationToken);
            if (todoItem is null || todoItem.OwnerId != user.Id || todoItem.Project is null || todoItem.Project.OwnerId != request.UserId)
                return Result.Failure("Task Not Found");

            var project = await _unitOfWork.ProjectRepository.GetProjectWithoutTasksAsync(todoItem.ProjectId, cancellationToken); 
            if(project is null)
                return Result.Failure("Task's Project Not Found");

            Guid assigneeId = Guid.Empty;
            bool hasAssigneeId = todoItem.AssigneeId is not null && todoItem.AssigneeId != Guid.Empty; 
            if (hasAssigneeId)
                assigneeId = todoItem.AssigneeId!.Value; 

            var projectDetailsKey = CacheKeys.ProjectDetailedViews(user.Id, project.Id); 

            try
            {
                _logger.LogInformation("Removing TodoItem From Project & Saving Changes"); 
                todoItem.Project.DeleteTodoItem(todoItem);

                await _unitOfWork.SaveChangesAsync(cancellationToken);

                _logger.LogInformation("Removing ProjectDetailsKey");
                await _cache.RemoveAsync(projectDetailsKey, cancellationToken);

                _logger.LogInformation("Sending Assignee info to deletionEventHandler");
                var deletionEvent = new AssignedTodoItemDeletedEvent(assigneeId);
                await _mediator.Publish(deletionEvent, cancellationToken);
            }

            catch (Exception)
            {
                return Result.Failure("Issue Deleting Task."); 
            }

            _logger.LogInformation("Returning Successful");
            return Result.Success();
        }
    }
}
