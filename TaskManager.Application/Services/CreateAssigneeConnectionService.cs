using Microsoft.AspNetCore.Identity;
using TaskManager.Application.DTOs;
using TaskManager.Application.Interfaces;
using TaskManager.Domain.Entities;

namespace TaskManager.Application.Services
{
    public class CreateAssigneeConnectionService : ICreateAssigneeConnectionService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly UserManager<User> _userManager;
        

        public CreateAssigneeConnectionService(IUnitOfWork unitOfWork, UserManager<User> userManager)
        {
            _unitOfWork = unitOfWork; 
            _userManager = userManager;

        }
        public async Task<CreateAssigneeConnectionResponse> CreateAssigneeConnectionAsync(CreateAssigneeConnectionRequest request)
        {
            //Validate request
            Console.WriteLine("In CreateAssigneeConnectionAsync");
            if(request is null)
            {
                return new CreateAssigneeConnectionResponse
                {
                    Success = false,
                    Message = "Error Encountered"
                }; 
            }

            if (request.UserId is null || request.UserId == Guid.Empty)
            {
                return new CreateAssigneeConnectionResponse
                {
                    Success = false,
                    Message = "Null UserId"
                };
            }

            //Verify user and assignee exist
            var user = await _userManager.FindByIdAsync(request.UserId.ToString());
            var assignee = await _userManager.FindByIdAsync(request.AssigneeId.ToString()); 

            if (user is null)
            {
                return new CreateAssigneeConnectionResponse
                {
                    Success = false,
                    Message = "User Not Found"
                };
            }

            if (assignee is null)
            {
                return new CreateAssigneeConnectionResponse
                {
                    Success = false,
                    Message = "No Account Found For Assignee"
                };
            }

            //Check if connection already exists
            if (await _unitOfWork.UserConnectionRepository.FindConnection(user.Id, assignee.Id))
            {
                Console.WriteLine("Already connected");
                return new CreateAssigneeConnectionResponse
                {
                    AlreadyConnected = true,
                    Success = false,
                    Message = "Already Connected",
                };
            }

            //Create connection
            var connection = new UserConnection
            {
                id = new Guid(),
                UserId = request.UserId ?? Guid.Empty,
                //User = user,
                AssigneeId = request.AssigneeId,
                //Assignee = assignee,
            };

            Console.WriteLine("Connection Created, not saved");

            try
            {
                _unitOfWork.UserConnectionRepository.Add(connection);
                await _unitOfWork.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error saving connection: {ex.Message}");
                return new CreateAssigneeConnectionResponse
                {
                    Success = false,
                    Message = "Error saving connection"
                };
            }

            //Return response

            var userConnectionDto = new UserConnectionDto
            {
                UserId = user.Id,
                AssigneeFirstName = assignee.FirstName,
                AssigneeLastName = assignee.LastName,
                AssigneeEmail = assignee.Email ?? string.Empty,
                AssigneeId = assignee.Id,
            };

            return new CreateAssigneeConnectionResponse
            {
                Success = true,
                Message = "UserConnection Made Successfully",
                Assignee = userConnectionDto
            }; 
        }
    }
}
