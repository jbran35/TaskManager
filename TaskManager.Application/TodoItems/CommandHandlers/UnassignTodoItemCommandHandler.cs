using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Caching.Distributed;
using TaskManager.Application.Common;
using TaskManager.Application.TodoItems.Commands;
using TaskManager.Domain.Common;
using TaskManager.Domain.Entities;
using TaskManager.Domain.Interfaces;

namespace TaskManager.Application.TodoItems.CommandHandlers
{
    //NOT USED CURRENTLY, but could be used to add quick unassignment options on the ProjectDetailedView page. 
    public class UnassignTodoItemCommandHandler(IUnitOfWork unitOfWork, IDistributedCache cache, UserManager<User> userManaager) : IRequestHandler<UnassignTodoItemCommand, Result>
    {
            private readonly IUnitOfWork _unitOfWork = unitOfWork;
            private readonly IDistributedCache _cache = cache;
            private readonly UserManager<User> _userManager = userManaager;
        public async Task<Result> Handle(UnassignTodoItemCommand request, CancellationToken cancellationToken)
        {
            var user = await _userManager.FindByIdAsync(request.UserId.ToString());
            if (user is null)
                return Result.Failure("User Not Found.");

            var project = await _unitOfWork.ProjectRepository.GetProjectWithoutTasksAsync(request.ProjectId, cancellationToken);
            var todoItem = await _unitOfWork.TodoItemRepository.GetTodoItemByIdAsync(request.TodoItemId, cancellationToken);

            if (project is null || todoItem is null || todoItem.ProjectId != project.Id || project.OwnerId != user.Id || todoItem.OwnerId != user.Id)
                return Result.Failure("Project or Task Not Found.");

            var assigneeKey = string.Empty;
            if (todoItem.AssigneeId is not null && todoItem.AssigneeId != Guid.Empty)
                assigneeKey = CacheKeys.AssignedTodoItems((Guid) todoItem.AssigneeId); 

            var result = todoItem.Unassign();

            if (result.IsFailure)
                return Result.Failure(result.ErrorMessage ?? "Failed To Unassign The Task");

            try
            {
                _unitOfWork.TodoItemRepository.Update(todoItem);
                await _unitOfWork.SaveChangesAsync(cancellationToken);

                if (assigneeKey != string.Empty)
                    await _cache.RemoveAsync(assigneeKey, cancellationToken);
            }
            catch (Exception)
            {
                return Result.Failure("Issue Unassigning Task");
            }

            return Result.Success();
        }
    }
}
