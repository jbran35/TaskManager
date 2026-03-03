using Microsoft.AspNetCore.Identity;
using System.Diagnostics.CodeAnalysis;
using TaskManager.Application.DTOs;
using TaskManager.Application.Interfaces;
using TaskManager.Domain.Entities;

namespace TaskManager.Application.Services
{
    public class GetAssignedTodoItemsService : IGetAssignedTodoItemsService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly UserManager<User> _userManager;
        public GetAssignedTodoItemsService(IUnitOfWork unitOfWork, UserManager<User> userManager)
        {
            _unitOfWork = unitOfWork;
            _userManager = userManager;
        }
        public async Task<GetAssignedTodoItemsResponse> GetAssignedTodoItemsAsync(Guid userId)
        {
            // Validate the request
            if (userId == Guid.Empty)
            {
                return new GetAssignedTodoItemsResponse
                {
                    Success = false,
                    Message = "Invalid request"
                };
            }

            // Retrieve user by UserId
            var user = await _userManager.FindByIdAsync(userId.ToString());

            if (user is null)
            {
                return new GetAssignedTodoItemsResponse
                {
                    Success = false,
                    Message = "User not found."
                };
            }

            // Pull tasks assigned to user
            var tasks = await _unitOfWork.TodoItemRepository.GetAssignedTodoItemsByUserIdAsync(userId);

            if (tasks is null || !tasks.Any())
            {
                return new GetAssignedTodoItemsResponse
                {
                    Success = true,
                    Message = "No tasks assigned.",
                    AssignedTasks = new List<TodoItemListEntryDto>()
                };
            }

            //Create TaskListEntryDto for each task
            var taskListEntries = tasks.Select(task => new TodoItemListEntryDto
            {
                ProjectId = task.ProjectId,
                ProjectName = task.Project.Name.Value,
                TodoItemId = task.Id,
                Title = task.Name.Value,
                AssigneeId = task.AssigneeId,
                AssigneeName = task.Assignee?.FullName ?? string.Empty,
                OwnerName = task.Owner.FullName, //Only passing owner's name 
                DueDate = task.DueDate,
                Status = task.Status,
                Priority = task.Priority
            }).ToList();

            //Generate Response
            return new GetAssignedTodoItemsResponse
            {
                Success = true,
                Message = "Tasks retrieved successfully.",
                AssignedTasks = taskListEntries
            };
        }
    }
}

