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
            //Validate reequeset
            if(request is null || request.TodoItemId == Guid.Empty || request.UserId == Guid.Empty)
                return Result.Failure("Invalid Request");

            //Check if user exists
            var user = await _userManager.FindByIdAsync(request.UserId.ToString());

            if(user is null)
                return Result.Failure("User Not Found");
           
            var todoItem = await _unitOfWork.TodoItemRepository.GetTodoItemByIdAsync(request.TodoItemId, cancellationToken);

            if (todoItem is null || todoItem.OwnerId != user.Id || todoItem.Project is null || todoItem.Project.OwnerId != request.UserId)
                return Result.Failure("Task Not Found");

            //Delete todo item & save changes
            try
            {
                _logger.LogInformation("Removing TodoItem From Project"); 
                todoItem.Project.DeleteTodoItem(todoItem);

                await _unitOfWork.SaveChangesAsync(cancellationToken);
                _logger.LogInformation("Removing TodoItem From Project Details & Tiles Redis");
                
                string detailsKey = CacheKeys.ProjectDetailedViews(user.Id, todoItem.ProjectId);
                string tilesKey = CacheKeys.ProjectTiles(user.Id); 

                await _cache.RemoveAsync(detailsKey, cancellationToken);
                await _cache.RemoveAsync(tilesKey, cancellationToken); 
            }

            catch(Exception)
            {
                return Result.Failure("Issue Deleting Task."); 
            }

            return Result.Success();
        }
    }
}
