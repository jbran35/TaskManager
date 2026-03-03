using Microsoft.AspNetCore.Identity;
using TaskManager.Application.DTOs;
using TaskManager.Application.Interfaces;
using TaskManager.Domain.Interfaces;


using TaskManager.Domain.Interfaces;

using TaskManager.Domain.Entities;


namespace TaskManager.Application.Services
{
    public class CompleteProjectService : ICompleteProjectService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly UserManager<User> _userManager;

        public CompleteProjectService(IUnitOfWork unitOfWork, UserManager<User> userManager)
        {
            _unitOfWork = unitOfWork;
            _userManager = userManager;

        }

        public async Task<CompleteProjectResponse> CompleteProjectAsync(Guid projectId, Guid userId)
        {
            // Check that Ids aren't empty Guids
            if(projectId == Guid.Empty || userId == Guid.Empty)
            {
                return new CompleteProjectResponse
                {
                    Success = false,
                    Message = "Need both the User's ID and Project's ID"
                };
            }

            //Check that the user exists
            var user = await _userManager.FindByIdAsync(userId.ToString());

            if (user is null)
            {
                return new CompleteProjectResponse
                {
                    Success = false,
                    Message = "UserId Not Found."
                };
            }

            // Load project from repository
            var project = await _unitOfWork.ProjectRepository.GetProjectByIdAsync(projectId);

            if (project is null)
            {
                return new CompleteProjectResponse
                {
                    Success = false,
                    Message = "Project Not Found."
                };
            }

            // Check that user is owner of project
            if (project.OwnerId != userId)
            {
                return new CompleteProjectResponse
                {
                    ProjectId = project.Id,
                    ProjectName = project.Name.Value,
                    Status = project.Status,
                    Success = false,
                    Message = "Current user is not the listed owner of the project."
                };
            }

            // Load all tasks for project
            var todoItems = await _unitOfWork.TodoItemRepository.GetTodoItemsByProjectIdAsync(projectId);
            
            if (todoItems is null || !todoItems.Any())
            {
                return new CompleteProjectResponse
                {
                    ProjectId = project.Id,
                    ProjectName = project.Name.Value,
                    Status = project.Status,
                    Success = false,
                    Message = "No tasks found for the project."
                };
            }

            // if any tasks are not complete, mark them as CompletePerProject (used for tasks completed while completing a project)
            var incompleteTodoItems = todoItems.Where(t => t.Status != Domain.Enums.Status.Complete).ToList();
            
            if (incompleteTodoItems.Any())
            {
                foreach (var todoItem in incompleteTodoItems)
                {
                    todoItem.MarkAsComplete();
                }
            }

            // Committ changes to tasks and project
            try
            {
                _unitOfWork.TodoItemRepository.Update(incompleteTodoItems);
                project.MarkAsComplete();
                _unitOfWork.ProjectRepository.Update(project);
                await _unitOfWork.SaveChangesAsync(); 
            }
            
            catch (Exception ex)
            {
                return new CompleteProjectResponse
                {
                    ProjectId = project.Id,
                    ProjectName = project.Name.Value,
                    Status = project.Status,
                    Success = false,
                    Message = "Could not complete the project: \n" + ex.Message
                };
            }

            var totalTodoItems = todoItems.Count();

            return new CompleteProjectResponse
            {
                ProjectId = project.Id,
                ProjectName = project.Name.Value,
                Status = project.Status,
                TotalTasks = totalTodoItems,
                Success = true,
                Message = "Project marked as complete successfully."
            };
        }
    }
}
