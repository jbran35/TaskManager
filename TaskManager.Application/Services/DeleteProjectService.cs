using Microsoft.AspNetCore.Identity;
using TaskManager.Application.DTOs;
using TaskManager.Application.Interfaces;
using TaskManager.Domain.Entities;

namespace TaskManager.Application.Services
{
    public class DeleteProjectService : IDeleteProjectService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly UserManager<User> _userManager;

        public DeleteProjectService(IUnitOfWork unitOfWork, UserManager<User> userManager)
        {
            _unitOfWork = unitOfWork;
            _userManager = userManager;
        }

        public async Task<DeleteProjectResponse> DeleteProjectAsync(Guid projectId, Guid userId)
        {
            //Validate Input
            if (projectId == Guid.Empty || userId == Guid.Empty)
            {
                return new DeleteProjectResponse
                {
                    Success = false,
                    Message = "Invalid Project or User ID."
                };
            }

            // Verify User exists
            var user = await _userManager.FindByIdAsync(userId.ToString());

            if (user is null)
            {
                return new DeleteProjectResponse
                {
                    Success = false,
                    Message = "User ID Not Found."
                };
            }

            // Verify Project exists
            var project = await _unitOfWork.ProjectRepository.GetProjectByIdAsync(projectId);
            if (project is null)
            {
                return new DeleteProjectResponse
                {
                    Success = false,
                    Message = "Project ID Not Found."
                };
            }

            // Verify that the project belongs to the user
            if (project.OwnerId != userId)
            {
                return new DeleteProjectResponse
                {
                    Success = false,
                    Message = "Project does not belong to the user."
                };
            }

            // Delete all tasks associated with the project
            var todoItems = await _unitOfWork.TodoItemRepository.GetTodoItemsByProjectIdAsync(projectId);

            try
            {
                if(todoItems != null)
                {
                    _unitOfWork.TodoItemRepository.Delete(todoItems);
                }
                _unitOfWork.ProjectRepository.Delete(projectId);
                await _unitOfWork.SaveChangesAsync(); 
            }

            catch (Exception ex)
            {
                return new DeleteProjectResponse
                {
                    Success = false,
                    Message = "Could not delete project and associated tasks: \n" + ex.Message
                };
            }

            // Return response
            return new DeleteProjectResponse
            {
                ProjectId = project.Id,
                ProjectName = project.Name.Value,
                Success = true,
                Message = "Project and associated tasks deleted successfully."
            };
        }
    }
}
