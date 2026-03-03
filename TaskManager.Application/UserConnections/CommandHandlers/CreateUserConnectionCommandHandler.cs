using MediatR;
using Microsoft.AspNetCore.Identity;
using TaskManager.Application.UserConnections.Commands;
using TaskManager.Application.UserConnections.DTOs;
using TaskManager.Domain.Common;
using TaskManager.Domain.Entities;
using TaskManager.Domain.Interfaces;

namespace TaskManager.Application.UserConnections.CommandHandlers
{
    public class CreateUserConnectionCommandHandler(IUnitOfWork unitOfWork, UserManager<User> userManager) : IRequestHandler<CreateUserConnectionCommand, Result<UserConnectionDto>>
    {
        private readonly IUnitOfWork _unitOfWork = unitOfWork;
        private readonly UserManager<User> _userManager = userManager;
        public async Task<Result<UserConnectionDto>> Handle(CreateUserConnectionCommand request, CancellationToken cancellationToken)
        {

            //Validate request
            if (request is null || request.UserId == Guid.Empty || request.AssigneeId == Guid.Empty)
                return Result<UserConnectionDto>.Failure("Invalid Request");

            //Check that user and assignee exist
            var user = await _userManager.FindByIdAsync(request.UserId.ToString());

            if (user is null)
                return Result<UserConnectionDto>.Failure("Account Not Found");

            var assignee = await _userManager.FindByIdAsync(request.AssigneeId.ToString());

            if (assignee is null)
                return Result<UserConnectionDto>.Failure("Assignee Not Found");

            //Ensure they are not already connected
            var areAlreadyConnected = await _unitOfWork.UserConnectionRepository.AnyConnectionExistsAsync(user.Id, assignee.Id, cancellationToken);

            if (areAlreadyConnected)
                return Result<UserConnectionDto>.Failure("It Looks Like You're Already Connected With This User");

            //Create the connection, add to users' accounts
            var newConnection = UserConnection.Create(user.Id, assignee.Id);

            try
            {
                _unitOfWork.UserConnectionRepository.Add(newConnection);
                await _unitOfWork.SaveChangesAsync(cancellationToken); 
            }

            catch (Exception)
            {
                return Result<UserConnectionDto>.Failure("Error Adding Assignee");
            }

            var connectionDto = new UserConnectionDto(
                newConnection.Id, 
                newConnection.UserId, 
                newConnection.AssigneeId, 
                assignee.FullName,
                assignee.Email ?? string.Empty); 


            return Result<UserConnectionDto>.Success(connectionDto); 
        }
    }
}
