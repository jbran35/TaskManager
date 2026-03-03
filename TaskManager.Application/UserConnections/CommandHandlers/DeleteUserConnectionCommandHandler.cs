using MediatR;
using Microsoft.AspNetCore.Identity;
using TaskManager.Application.UserConnections.Commands;
using TaskManager.Domain.Common;
using TaskManager.Domain.Entities;
using TaskManager.Domain.Interfaces;

namespace TaskManager.Application.UserConnections.CommandHandlers
{
    public class DeleteUserConnectionCommandHandler(IUnitOfWork unitOfWork, UserManager<User> userManager) : IRequestHandler<DeleteUserConnectionCommand, Result>
    {
        private readonly IUnitOfWork _unitOfWork = unitOfWork;
        private readonly UserManager<User> _userManager = userManager;
        public async Task<Result> Handle(DeleteUserConnectionCommand request, CancellationToken cancellationToken)
        {
            //Validate Request
            if (request is null || request.ConnectionId == Guid.Empty || request.UserId == Guid.Empty)
                return Result.Failure("Invalid Request");

            //Validate User
            var user = await _userManager.FindByIdAsync(request.UserId.ToString());

            if (user is null)
                return Result.Failure("Account Not Found");

            //Validate User Connections
            var connection = await _unitOfWork.UserConnectionRepository.GetConnectionByIdAsync(request.ConnectionId, cancellationToken);


            if (connection is null || connection.UserId != request.UserId)
                return Result.Failure("Issue Loading Assignee Connection");

            //Retrieve all todoItems owned by the user and assigneed to the assignee > Unassign tasks 
           // var itemsToUnassign = await _unitOfWork.TodoItemRepository.GetMyAssignedTodoItemsAsync(request.UserId, cancellationToken);



            //Remove Connection & Save
            _unitOfWork.UserConnectionRepository.Delete(connection);

            try
            {
                await _unitOfWork.SaveChangesAsync(cancellationToken); 
            }

            catch (Exception)
            {
                return Result.Failure("Issue Deleting Assignee"); 
            }

            return Result.Success("Deleted Successfully!");
        }
    }
}
