using Microsoft.AspNetCore.Identity;
using TaskManager.Application.DTOs;
using TaskManager.Application.Interfaces;
using TaskManager.Domain.Entities;

namespace TaskManager.Application.Services
{
    public class CreateProjectService
        : ICreateProjectService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly UserManager<User> _userManager;

        public CreateProjectService(IUnitOfWork unitOfWork, UserManager<User> userManager)
        {
            _unitOfWork = unitOfWork;
            _userManager = userManager;
        }
        public async Task<CreateProjectResponse> CreateProjectAsync(CreateProjectRequest request, Guid userId)
        {
            // Validate request and userId
            if (userId == Guid.Empty || request is null)
            {
                return new CreateProjectResponse
                {
                    Success = false,
                    Message = "Invalid request or user ID."
                };
            }

            // Retrieve user
            var user = await _userManager.FindByIdAsync(userId.ToString());

            // Check that user exists
            if (user is null)
            {
                return new CreateProjectResponse
                {
                    Success = false,
                    Message = "User ID Not Found."
                };
            }

            // Create new project entity
            var project = new Project(request.Name, userId, request.Description);

            // Save project 
            try
            {
                _unitOfWork.ProjectRepository.Add(project);
                await _unitOfWork.SaveChangesAsync();

            }
            catch (Exception ex)
            {
                return new CreateProjectResponse
                {
                    Success = false,
                    Message = "Could not create project: \n" + ex.Message,
                };
            }

            //Create return Dto, add it to the response dto, and return
            var projectTileDto = new ProjectTileDto
            {
                ProjectId = project.Id,
                ProjectName = project.Name.Value,
                Status = project.Status,
                TotalTodoItems = 0,
                CompletedTodoItems = 0,
                CompletePercentage = 0
            };


            return new CreateProjectResponse
            {
                CreatedProject = projectTileDto,
                Success = true,
                Message = "Project created successfully."
            };
        }
    }
}
