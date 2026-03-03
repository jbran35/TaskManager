using Microsoft.AspNetCore.Identity;
using TaskManager.Application.DTOs;
using TaskManager.Application.Interfaces;
using TaskManager.Domain.Entities;

namespace TaskManager.Application.Services
{
    public class UpdateTodoItemProjectAssignmentService : IUpdateTodoItemProjectAssignmentService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly UserManager<User> _userManager;
        public UpdateTodoItemProjectAssignmentService(IUnitOfWork unitOfWork, UserManager<User> userManager)
        {
            _unitOfWork = unitOfWork;
            _userManager = userManager;
        }

        public async Task<UpdateTodoItemProjectAssignmentResponse> UpdateTodoItemProjectAssignmentAsync(UpdateTodoItemProjectAssignmentRequest request, Guid userId, Guid todoItemId)
        {
            //Verify request & input
            if (userId == Guid.Empty || request.ProjectId == Guid.Empty || request.NewProjectId == Guid.Empty 
                || todoItemId == Guid.Empty)
            {
                return new UpdateTodoItemProjectAssignmentResponse
                {
                    Success = false,
                    Message = "Invalid request."
                };
            }

            //Check if user exists
            var user = await _userManager.FindByIdAsync(userId.ToString());
            if (user is null)
            {
                return new UpdateTodoItemProjectAssignmentResponse
                {
                    Success = false,
                    Message = "User not found."
                };
            }

            //Check if current & new projects exist
            var currProject = await _unitOfWork.ProjectRepository.GetProjectByIdAsync(request.ProjectId);
            var newProject = await _unitOfWork.ProjectRepository.GetProjectByIdAsync(request.NewProjectId);
            if (currProject is null || newProject is null)
            {
                return new UpdateTodoItemProjectAssignmentResponse
                {
                    Success = false,
                    Message = "Project not found."
                };
            }

            //Check if user is the owner of the current and new projects
            if (currProject.OwnerId != userId || newProject.OwnerId != userId)
            {
                return new UpdateTodoItemProjectAssignmentResponse
                {
                    Success = false,
                    Message = "User must be owner of both the old and new projects."
                };
            }

            // Check if task exists
            var task = await _unitOfWork.TodoItemRepository.GetTodoItemByIdAsync(todoItemId);

            if (task is null)
            {
                return new UpdateTodoItemProjectAssignmentResponse
                {
                    Success = false,
                    Message = "Task not found."
                };
            }

            // Update task's project assignment
            try
            {
                task.UpdateProjectAssignment(request.NewProjectId);
                _unitOfWork.TodoItemRepository.Update(task);
                await _unitOfWork.SaveChangesAsync();
            }

            catch (Exception ex)
            {
                return new UpdateTodoItemProjectAssignmentResponse
                {
                    Success = false,
                    Message = "Unable to update task project assignemnt: \n" + ex.Message
                };
            }

            return new UpdateTodoItemProjectAssignmentResponse
            {
                TodoItemId = task.Id,
                NewProjectId = newProject.Id,
                NewProjectTitle = newProject.Name.Value,
                Success = true,
                Message = "Task project assignment updated successfully."
            };
        }
    }
}
