using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using TaskManager.Application.Common;
using TaskManager.Application.TodoItems.Commands;
using TaskManager.Domain.Common;
using TaskManager.Domain.Entities;
using TaskManager.Domain.Interfaces;

namespace TaskManager.Application.TodoItems.CommandHandlers
{
    public class DeleteTodoItemCommandHandler(IUnitOfWork unitOfWork, UserManager<User> userManager, IDistributedCache cache, ILogger<DeleteTodoItemCommandHandler> logger) : IRequestHandler<DeleteTodoItemCommand, Result>
    {
        private readonly IUnitOfWork _unitOfWork = unitOfWork;
        private readonly UserManager<User> _userManager = userManager;
        private readonly IDistributedCache _cache = cache;
        private readonly ILogger<DeleteTodoItemCommandHandler> _logger = logger; 
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

            var projectDetailsKey = CacheKeys.ProjectDetailedViews(user.Id, project.Id); 

            var assigneeKey = string.Empty;
            if (todoItem.AssigneeId is not null && todoItem.AssigneeId != Guid.Empty)
                assigneeKey = CacheKeys.AssignedTodoItems((Guid)todoItem.AssigneeId);

            try
            {
                _logger.LogInformation("Removing TodoItem From Project"); 
                todoItem.Project.DeleteTodoItem(todoItem);

                await _unitOfWork.SaveChangesAsync(cancellationToken);
                _logger.LogInformation("Removing AssigneeId");

                await _cache.RemoveAsync(projectDetailsKey, cancellationToken); 

                if (assigneeKey != string.Empty)
                    await _cache.RemoveAsync(assigneeKey, cancellationToken);
            }

            catch(Exception)
            {
                return Result.Failure("Issue Deleting Task."); 
            }

            return Result.Success();
        }
    }
}
