using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using TaskManager.Application.TodoItems.Commands;
using TaskManager.Domain.Common;
using TaskManager.Domain.Entities;
using TaskManager.Domain.Interfaces;

namespace TaskManager.Application.TodoItems.CommandHandlers
{
    //NOT USED CURRENTLY, but could be used to add quick assignment options on the ProjectDetailedView page. 
    public class AssignTodoItemCommandHandler(IUnitOfWork unitOfWork, UserManager<User> userManager, 
        ILogger<AssignTodoItemCommandHandler> logger) 
        : IRequestHandler<AssignTodoItemCommand, Result>
    {
        private readonly IUnitOfWork _unitOfWork = unitOfWork;
        private readonly UserManager<User> _userManager = userManager;
        private readonly ILogger<AssignTodoItemCommandHandler> _logger = logger;
        public async Task<Result> Handle(AssignTodoItemCommand request, CancellationToken cancellationToken)
        {
            _logger.LogInformation("In Handler"); 
            var user = await _userManager.FindByIdAsync(request.UserId.ToString());
            if (user is null)
                return Result.Failure("User Not Found.");

            var todoItem = await _unitOfWork.TodoItemRepository.GetTodoItemByIdAsync(request.TodoItemId, cancellationToken);
            if (todoItem is null || todoItem.Project is null || todoItem.OwnerId != request.UserId || todoItem.Project.OwnerId != request.UserId)
                return Result.Failure("Task Or Project Not Found.");

            var assignee = await _userManager.FindByIdAsync(request.AssigneeId.ToString());
            if (assignee is null)
                return Result.Failure("Assignee Not Found.");

            _logger.LogInformation("Assigning To User");
            todoItem.AssignToUser(request.AssigneeId);

            try
            {
                _logger.LogInformation("Saving Changes");
                _unitOfWork.TodoItemRepository.Update(todoItem);
                await _unitOfWork.SaveChangesAsync(cancellationToken);
            }

            catch (Exception ex)
            {
                _logger.LogError(ex, "Issue Assigning Task");
                return Result.Failure("Issue Assigning Task");
            }

            return Result.Success();
        }
    }
}
