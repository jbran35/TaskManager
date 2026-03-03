using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Caching.Distributed;
using TaskManager.Application.TodoItems.Commands;
using TaskManager.Domain.Common;
using TaskManager.Domain.Entities;
using TaskManager.Domain.Interfaces;

namespace TaskManager.Application.TodoItems.CommandHandlers
{
    public class AssignTodoItemCommandHandler(IUnitOfWork unitOfWork, UserManager<User> userManager, IDistributedCache cache) : IRequestHandler<AssignTodoItemCommand, Result>
    {
        private readonly IUnitOfWork _unitOfWork = unitOfWork;
        private readonly UserManager<User> _userManager = userManager;
        private readonly IDistributedCache _cache = cache; 
        public async Task<Result> Handle(AssignTodoItemCommand request, CancellationToken cancellationToken)
        {
            //Validate the request
            if (request is null || request.UserId == Guid.Empty || request.ProjectId == Guid.Empty || request.AssigneeId == Guid.Empty)
                return Result.Failure("Invalid Request.");

            //Check if the user exists
            var user = await _userManager.FindByIdAsync(request.UserId.ToString());
            if (user is null)
                return Result.Failure("User Not Found.");

            //Check if the user has permission to assign the todo item & access to the project
            var todoItem = await _unitOfWork.TodoItemRepository.GetTodoItemByIdAsync(request.TodoItemId, cancellationToken);
    

            if (todoItem is null || todoItem.Project is null || todoItem.OwnerId != request.UserId || todoItem.Project.OwnerId != request.UserId)
                return Result.Failure("Task Or Project Not Found.");

            //Check if the assignee exists
            var assignee = await _userManager.FindByIdAsync(request.AssigneeId.ToString());

            if (assignee is null)
                return Result.Failure("Assignee Not Found.");

            //Assign the todo item to the user
            todoItem.AssignToUser(request.AssigneeId);

            //Save changes
            try
            {
                _unitOfWork.TodoItemRepository.Update(todoItem);
                await _unitOfWork.SaveChangesAsync(cancellationToken);
            }

            catch (Exception)
            {
                return Result.Failure("Issue Assigning Task");
            }

            return Result.Success();
        }
    }
}
