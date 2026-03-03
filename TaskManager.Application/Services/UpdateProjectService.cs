using Microsoft.AspNetCore.Identity;
using TaskManager.Application.DTOs;
using TaskManager.Application.Interfaces;
using TaskManager.Domain.Entities;

namespace TaskManager.Application.Services
{
    public class UpdateProjectService : IUpdateProjectService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly UserManager<User> _userManager;

        public UpdateProjectService(IUnitOfWork unitOfWork, UserManager<User> userManager)
        {
            _unitOfWork = unitOfWork;
            _userManager = userManager;
        }
        public async Task<UpdateProjectResponse> UpdateProjectAsync(UpdateProjectRequest request, Guid projectId, Guid userId)
        {
            //Verify userId 
            if (userId == Guid.Empty)
            {
                return new UpdateProjectResponse
                {
                    Success = false,
                    Message = "Invalid UserId."
                };
            }

            //Verify projectId
            if (projectId == Guid.Empty)
            {
                return new UpdateProjectResponse
                {
                    Success = false,
                    Message = "Invalid ProjectId."
                };
            }

            //Check if user exists
            var user = await _userManager.FindByIdAsync(userId.ToString());
            
            if (user is null)
            {
                return new UpdateProjectResponse
                {
                    Success = false,
                    Message = "User not found."
                };
            }

            //Check if project exists
            var project = await _unitOfWork.ProjectRepository.GetProjectByIdAsync(projectId);
            
            if (project is null)
            {
                return new UpdateProjectResponse
                {
                    Success = false,
                    Message = "Project not found."
                };
            }

            //Check if user is the owner of the project
            if (project.OwnerId != userId)
            {
                return new UpdateProjectResponse
                {
                    Success = false,
                    Message = "User is not owner of the project."
                };
            }


            //Update project details
            project.UpdateTitle(request.ProjectName);

            if (!string.IsNullOrWhiteSpace(request.Description))
            {
                project.UpdateDescription(request.Description);
            }

            try
            {
                _unitOfWork.ProjectRepository.Update(project);
                await _unitOfWork.SaveChangesAsync(); // Commit the transaction
            }
            catch (Exception ex)
            {
                return new UpdateProjectResponse
                {
                    Success = false,
                    Message = "Error updating project: \n" + ex.Message
                };
            }
            
            //Get task counts & Generate response
            int totalTodoItemCount = (int)await _unitOfWork.TodoItemRepository.GetProjectTodoItemCountAsync(projectId);
            int completedTodoItemCount;
            double percentComplete;

            if (totalTodoItemCount == 0)
            {
                completedTodoItemCount = 0;
                percentComplete = 0;
            }

            else
            {
                completedTodoItemCount = (int)await _unitOfWork.TodoItemRepository.GetProjectCompletedTodoItemCountAsync(projectId);
                percentComplete = completedTodoItemCount / totalTodoItemCount;
            }

            return new UpdateProjectResponse
            {
                Success = true,
                Message = "Project updated successfully.",
                ProjectDetailViewDto = new ProjectDetailedViewDto
                {
                    //ProjectId, ProjectName, Status, Description, TotalTodoItemsCount, CompletedTodoItemsCount, PercentComplete, List of Tasks (TaskListEntryDto)

                    ProjectId = project.Id,
                    ProjectName = project.Name.Value,
                    Status = project.Status,
                    Description = project.Description?.Value,
                    TotalTodoItems = totalTodoItemCount,
                    CompletedTodoItems = completedTodoItemCount,
                    PercentComplete = percentComplete,
                }
            };
        }
    }
}