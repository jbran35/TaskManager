using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using TaskManager.Application.TodoItems.Events;
using TaskManager.Application.UserConnections.Commands;
using TaskManager.Domain.Common;
using TaskManager.Domain.Entities;
using TaskManager.Domain.Interfaces;

namespace TaskManager.Application.UserConnections.CommandHandlers
{
    public class DeleteUserConnectionCommandHandler(IUnitOfWork unitOfWork, UserManager<User> userManager,
        ILogger<DeleteUserConnectionCommandHandler> logger, IMediator mediator) : IRequestHandler<DeleteUserConnectionCommand, Result>
    {
        private readonly IUnitOfWork _unitOfWork = unitOfWork;
        private readonly UserManager<User> _userManager = userManager;
        private readonly ILogger<DeleteUserConnectionCommandHandler> _logger = logger;
        private readonly IMediator _mediator = mediator;
        public async Task<Result> Handle(DeleteUserConnectionCommand request, CancellationToken cancellationToken)
        {
            _logger.LogInformation("In Handler"); 

            var user = await _userManager.FindByIdAsync(request.UserId.ToString());
            if (user is null)
                return Result.Failure("Account Not Found");

            var connection = await _unitOfWork.UserConnectionRepository.GetConnectionByIdAsync(request.ConnectionId, cancellationToken);
            if (connection is null || connection.UserId != request.UserId)
                return Result.Failure("Issue Loading Assignee Connection");

            var assignee = await _userManager.FindByIdAsync(connection.AssigneeId.ToString()); 
            if(assignee is null)
                return Result.Failure("User Not Found");

            var assigneeId = assignee.Id; 

            //Unassigning tasks that were assigned to the removed user
            var taskIdsToUnassign = await _unitOfWork.TodoItemRepository.GetMyTodoItemsAssignedToUser(user.Id, connection.AssigneeId, cancellationToken);

            bool itemsWereUnassigned = false;
            try
            {
                if (taskIdsToUnassign is not null && taskIdsToUnassign.Count != 0)
                {
                    _logger.LogInformation("Unassigning Tasks");
                    itemsWereUnassigned = true;
                    await _unitOfWork.TodoItemRepository.UnassignTasksByIdAsync(taskIdsToUnassign, cancellationToken);
                }

                _logger.LogInformation("Deleting & Saving Changes");
                _unitOfWork.UserConnectionRepository.Delete(connection);
                await _unitOfWork.SaveChangesAsync(cancellationToken);

                if (itemsWereUnassigned)
                {
                    _logger.LogInformation("Sending Assignee info to deletionEventHandler");
                    var deletionEvent = new AssignedTodoItemDeletedEvent(assigneeId);
                    await _mediator.Publish(deletionEvent, cancellationToken);
                }

                return Result.Success("Deleted Successfully!");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Issue Deleting User Connection");
                return Result.Failure("Issue Deleting Assignee"); 
            }
        }
    }
}
