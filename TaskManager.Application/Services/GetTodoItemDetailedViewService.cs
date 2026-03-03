using Microsoft.AspNetCore.Identity;
using TaskManager.Application.DTOs;
using TaskManager.Application.Interfaces;
using TaskManager.Domain.Entities;

namespace TaskManager.Application.Services
{
    public class GetTodoItemDetailedViewService : IGetTodoItemDetailedViewService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly UserManager<User> _userManager;

        public GetTodoItemDetailedViewService(IUnitOfWork unitOfWork, UserManager<User> userManager)
        {
            _unitOfWork = unitOfWork;
            _userManager = userManager;
        }

        public async Task<GetTodoItemDetailedViewResponse> GetTodoItemDetailedViewAsync(Guid todoItemId, Guid userId)
        {
            //Validate Input
            if (userId == Guid.Empty)
            {
                return new GetTodoItemDetailedViewResponse
                {
                    Success = false,
                    Message = "User ID is required to get task detailed view"
                };
            }

            if (todoItemId == Guid.Empty)
            {
                return new GetTodoItemDetailedViewResponse
                {
                    Success = false,
                    Message = "Task ID is required to get task detailed view"
                };
            }

            //Retrieve Task, Ensure It Exissts
            var todoItem = await _unitOfWork.TodoItemRepository.GetTodoItemByIdAsync(todoItemId);

            if (todoItem is null)
            {
                return new GetTodoItemDetailedViewResponse
                {
                    Success = false,
                    Message = "Task not found"
                };
            }

            //Retrieve Task's Project Owner, Ensure It Exists
            var projectOwnerId = await _unitOfWork.ProjectRepository.GetProjectOwnerIdAsync(todoItem.ProjectId);

            if (projectOwnerId is null || projectOwnerId == Guid.Empty)
            {
                return new GetTodoItemDetailedViewResponse
                {
                    Success = false,
                    Message = "Could not find task's project"
                };
            }

            //Retrieve user, Ensure It Exists and Has Permission to View Task (Must Be Project Owner or Task Assignee)
            var user = await _userManager.FindByIdAsync(userId.ToString());
            if (user is null)
            {
                return new GetTodoItemDetailedViewResponse
                {
                    Success = false,
                    Message = "User not found"
                };
            }

            if (projectOwnerId != userId && userId != todoItem.AssigneeId)
            {
                return new GetTodoItemDetailedViewResponse
                {
                    Success = false,
                    Message = "You do not have permission to view this task"
                };
            }

            //Map Task to TaskDetailsDto
            var todoItemDetails = new TodoItemDetailsDto
            {
                TodoItemId = todoItem.Id,
                Name = todoItem.Name.Value ?? string.Empty,
                ProjectId = todoItem.ProjectId,
                ProjectName = todoItem.Project?.Name?.Value ?? string.Empty,
                Description = todoItem.Description?.Value,
                AssigneeId = todoItem.AssigneeId,
                AssigneeName = todoItem.Assignee?.FullName ?? string.Empty,
                AssigneeEmail = todoItem.Assignee?.Email ?? string.Empty,
                DueDate = todoItem.DueDate,
                Status = todoItem.Status,
                Priority = todoItem.Priority
            };

            //Return Response
            return new GetTodoItemDetailedViewResponse
            {
                Success = true,
                Message = "Task details retrieved successfully",
                TodoItemDetails = todoItemDetails
            };
        }
    }
}
