using MediatR;
using Microsoft.AspNetCore.Identity;
using TaskManager.Application.TodoItems.Commands;
using TaskManager.Domain.Common;
using TaskManager.Domain.Entities;
using TaskManager.Domain.Interfaces;

namespace TaskManager.Application.TodoItems.CommandHandlers
{
    public class UnassignTodoItemCommandHandler(IUnitOfWork unitOfWork, UserManager<User> userManaager) : IRequestHandler<UnassignTodoItemCommand, Result>
    {
            private readonly IUnitOfWork _unitOfWork = unitOfWork;
            private readonly UserManager<User> _userManager = userManaager;
        public async Task<Result> Handle(UnassignTodoItemCommand request, CancellationToken cancellationToken)
        {
            // Validate request
            if (request is null || request.ProjectId == Guid.Empty || request.UserId == Guid.Empty || request.TodoItemId == Guid.Empty)
                return Result.Failure("Invalid Request.");

            // Check if user exists
            var user = await _userManager.FindByIdAsync(request.UserId.ToString());

            if (user is null)
                return Result.Failure("User Not Found.");

            var project = await _unitOfWork.ProjectRepository.GetProjectWithoutTasksAsync(request.ProjectId, cancellationToken);
            var todoItem = await _unitOfWork.TodoItemRepository.GetTodoItemByIdAsync(request.TodoItemId, cancellationToken);

            // Validate project & todo item existence, ownership
            if (project is null || todoItem is null || todoItem.ProjectId != project.Id || project.OwnerId != user.Id || todoItem.OwnerId != user.Id)
                return Result.Failure("Project or Task Not Found.");

            // Unassign the todo item & save changes
            var result = todoItem.Unassign();

            if (result.IsFailure)
                return Result.Failure(result.ErrorMessage ?? "Failed To Unassign The Task");

            try
            {
                _unitOfWork.TodoItemRepository.Update(todoItem);
                await _unitOfWork.SaveChangesAsync(cancellationToken);
            }
            catch (Exception)
            {
                return Result.Failure("Issue Unassigning Task");
            }

            return Result.Success();
        }
    }
}
