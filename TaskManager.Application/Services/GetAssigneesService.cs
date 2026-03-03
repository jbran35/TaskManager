using Microsoft.AspNetCore.Identity;
using TaskManager.Application.DTOs;
using TaskManager.Application.Interfaces;
using TaskManager.Domain.Entities;

namespace TaskManager.Application.Services
{
    public class GetAssigneesService : IGetAssigneesService
    {
        private readonly UserManager<User> _userManager;
        private readonly IUnitOfWork _unitOfWork;

        public GetAssigneesService(UserManager<User> userManager, IUnitOfWork unitOfWork)
        {
            _userManager = userManager;
            _unitOfWork = unitOfWork;
        }

        public async Task<GetAssigneesResponse> GetAssigneesAsync(Guid userId)
        {
            // Validate input
            if (userId == Guid.Empty)
            {
                return new GetAssigneesResponse
                {
                    Success = false,
                    Message = "Invalid request"
                };
            }

            // Retrieve user by UserId
            var user = await _userManager.FindByIdAsync(userId.ToString());

            if (user is null)
            {
                return new GetAssigneesResponse
                {
                    Success = false,
                    Message = "User not found."
                };
            }

            // Get user connections
            var connections = await _unitOfWork.UserConnectionRepository.GetConnectionsAsync(user.Id);

            if (connections is null)
            {
                return new GetAssigneesResponse
                {
                    Success = false,
                    Message = "No assignees found."
                };
            }

            //Create list of UserConnectionDto to return
            List<UserConnectionDto> userConnectionDtos = new List<UserConnectionDto>();

            foreach (var connection in connections)
            {
                var assignee = await _userManager.FindByIdAsync(connection.AssigneeId.ToString());

                if (assignee is null)
                {                    
                    continue; 
                }

                userConnectionDtos.Add(new UserConnectionDto
                {
                    UserId = connection.UserId,
                    AssigneeId = connection.AssigneeId,
                    AssigneeFirstName = assignee.FirstName ?? string.Empty,
                    AssigneeLastName = assignee.LastName ?? string.Empty,
                    AssigneeEmail = assignee.Email ?? string.Empty
                });
            }

            return new GetAssigneesResponse
            {
                Assignees = userConnectionDtos,
                Success = true,
                Message = "Assignees retrieved successfully."
            };
        }
    }
}
