using Microsoft.AspNetCore.Identity;
using TaskManager.Application.DTOs;
using TaskManager.Application.Interfaces;
using TaskManager.Domain.Entities;
using TaskManager.Domain.Enums;

namespace TaskManager.Application.Services
{
    public class GetProjectDetailedViewService : IGetProjectDetailedViewService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly UserManager<User> _userManager;

        public GetProjectDetailedViewService(IUnitOfWork unitOfWork, UserManager<User> userManager)
        {
            _unitOfWork = unitOfWork;
            _userManager = userManager;

        }

        public async Task<GetProjectDetailedViewResponse> GetProjectDetailedViewAsync(Guid projectId, Guid userId)
        {
            // Validate Input

            if (projectId == Guid.Empty || userId == Guid.Empty)
            {
                return new GetProjectDetailedViewResponse
                {
                    Success = false,
                    Message = "Project ID & UserId are required."
                };
            }

            //Ensure the project exists

            var project = await _unitOfWork.ProjectRepository.GetProjectByIdAsync(projectId);
            if (project is null)
            {
                return new GetProjectDetailedViewResponse
                {
                    Success = false,
                    Message = "Project not found."
                };
            }

            // Retrieve user and ensure they are the project owner
            var user = await _userManager.FindByIdAsync(userId.ToString());
            if (user is null)
            {
                return new GetProjectDetailedViewResponse
                {
                    Success = false,
                    Message = "User not found."
                };
            }

            if (project.OwnerId != user.Id)
            {
                return new GetProjectDetailedViewResponse
                {
                    Success = false,
                    Message = "You do not have permission to view this project."
                };
            }

            // Retrieve tasks associated with the project, ensure they exists
            var tasks = await _unitOfWork.TodoItemRepository.GetTodoItemsByProjectIdAsync(projectId);

            if (tasks is null)
            {
                return new GetProjectDetailedViewResponse
                {
                    Success = true,
                    Message = "No tasks"
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
                DueDate = task.DueDate,
                Status = task.Status,
                Priority = task.Priority
            }).ToList();

            // Create the ProjectDetailedViewDto
            var completedTasks = tasks.Count(t => t.Status == Status.Complete);
            var totalTasks = tasks.Count();
            var percentComplete = 0;

            if (totalTasks > 0)
            {
                percentComplete = (int)((decimal)completedTasks / totalTasks * 100);
            }

            var projectDetailsDto = new ProjectDetailedViewDto
            {
                ProjectId = project.Id,
                ProjectName = project.Name.Value,
                Status = project.Status,
                Description = project.Description?.Value,
                TotalTodoItems = totalTasks,
                CompletedTodoItems = completedTasks,
                PercentComplete = percentComplete,
            };

            // Create Response
            return new GetProjectDetailedViewResponse
            {
                Success = true,
                Message = "Project details retrieved successfully.",
                ProjectDetails = projectDetailsDto
            };
        }
    }
}