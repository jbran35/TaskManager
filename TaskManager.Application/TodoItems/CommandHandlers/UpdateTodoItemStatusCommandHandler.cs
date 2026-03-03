using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Caching.Distributed;
using TaskManager.Application.Common;
using TaskManager.Application.Interfaces;
using TaskManager.Application.TodoItems.Commands;
using TaskManager.Application.TodoItems.DTOs;
using TaskManager.Application.TodoItems.DTOs.Responses;
using TaskManager.Domain.Common;
using TaskManager.Domain.Entities;
using TaskManager.Domain.Enums;
using TaskManager.Domain.Interfaces;


namespace TaskManager.Application.TodoItems.CommandHandlers
{
    public class UpdateTodoItemStatusCommandHandler(IUnitOfWork unitOfWork, UserManager<User> userManager, ITodoItemUpdateNotificationService updateService, IDistributedCache cache) : IRequestHandler<UpdateTodoItemStatusCommand, Result<TodoItemEntry>>
    {
        private readonly IUnitOfWork _unitOfWork = unitOfWork;
        private readonly UserManager<User> _userManager = userManager;
        private readonly ITodoItemUpdateNotificationService _updateService = updateService;
        private readonly IDistributedCache _cache = cache; 
        public async Task<Result<TodoItemEntry>> Handle(UpdateTodoItemStatusCommand request, CancellationToken cancellationToken)
        {
            //Validate request 
            if(request is null || request.UserId == Guid.Empty || request.TodoItemId == Guid.Empty)
            {
                return Result<TodoItemEntry>.Failure("Invalid Request");
            }

            //Check if user exists
            var user = await _userManager.FindByIdAsync(request.UserId.ToString());
            if(user is null)
                return Result<TodoItemEntry>.Failure("User Not Found");

            //Check if project & task exist, belong to user, and are not deleted
            var todoItem = await _unitOfWork.TodoItemRepository.GetTodoItemByIdAsync(request.TodoItemId, cancellationToken);

            if(todoItem is null || todoItem.OwnerId != request.UserId || todoItem.Project is null || todoItem.Project.OwnerId != request.UserId)
                return Result<TodoItemEntry>.Failure("You Do Not Have Access To This Project Or Task");


            if(todoItem.Project.Status == Status.Deleted)
                return Result<TodoItemEntry>.Failure("This Project Has Been Deleted");
            

            //Complete the task
            if (todoItem.Status == Status.Incomplete)
                todoItem.MarkAsComplete();

            else if (todoItem.Status == Status.Complete)
            {
                todoItem.MarkAsIncomplete();
                todoItem.Project.MarkAsIncomplete();
            }

            try
            {
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

                if (todoItem.AssigneeId is not null && todoItem.AssigneeId != Guid.Empty)
                    await _updateService.NotifyTodoItemUpdated(todoItem.AssigneeId.ToString());

                var detailsKey = CacheKeys.ProjectDetailedViews(user.Id, todoItem.ProjectId);
                var tilesKey = CacheKeys.ProjectTiles(user.Id);

                await _cache.RemoveAsync(detailsKey, cancellationToken);
                await _cache.RemoveAsync(tilesKey, cancellationToken); 

                return Result<TodoItemEntry>.Success(listEntryDto);
            }
            catch (Exception)
            {
                return Result<TodoItemEntry>.Failure("Error Marking Task As Complete.");
            }
        }
    }
}
