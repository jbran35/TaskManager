using Microsoft.AspNetCore.Identity;
using TaskManager.Application.DTOs;
using TaskManager.Application.Interfaces;
using TaskManager.Domain.Entities;


namespace TaskManager.Application.Services
{
    public class GetUserProjectsService : IGetUserProjectsService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly UserManager<User> _userManager;

        public GetUserProjectsService(IUnitOfWork unitOfWork, UserManager<User> userManager)
        {
            _unitOfWork = unitOfWork;
            _userManager = userManager;
        }
        public async Task<GetUserProjectsResponse> GetUserProjectsAsync(Guid userId)
        {
            // Validate input
            if (userId == Guid.Empty)
            {
                return new GetUserProjectsResponse
                {
                    Success = false,
                    Message = "Guid empty in request",
                };
            }

            // Check if the user exists
            var user = await _userManager.FindByIdAsync(userId.ToString());
            if (user is null)
            {
                return new GetUserProjectsResponse
                {
                    Success = false,
                    Message = "User not found**"
                };
            }

            //Retrieve the user's projects (as ProjectTileDtos)
            var userProjects = await _unitOfWork.ProjectRepository.GetAllProjectsByUserIdAsync(userId);


            // If no projects found, return message
            if (userProjects is null || !userProjects.Any())
            {
                return new GetUserProjectsResponse
                {
                    Success = true,
                    Message = "No projects found",
                };
            }

            // Extract project IDs from the user's projects
            var projectIds = userProjects.Select(p => p.Id);

            //Retrieves the total and completed todo item counts for each project in a single query
            var projectCounts = await _unitOfWork.TodoItemRepository.GetProjectTodoItemCounts(projectIds);

            // Map the projects to ProjectTileDtos
            var projectTileDtos = userProjects.Select(project =>
            {
                var (totalTodoItems, completeTodoItems) = projectCounts.GetValueOrDefault(project.Id, (0, 0));

                double completedPercentage = 0.00;
                if (totalTodoItems > 0)
                {
                    completedPercentage = Math.Round((double)completeTodoItems / totalTodoItems, 2);
                }

                return new ProjectTileDto
                {
                    ProjectId = project.Id,
                    ProjectName = project.Name.Value,
                    Status = project.Status,
                    CreatedOn = project.CreatedOn,
                    TotalTodoItems = totalTodoItems,
                    CompletedTodoItems = completeTodoItems,
                    CompletePercentage = completedPercentage,
                };
            }).ToList();

            // Return the list of ProjectTileDtos in the response
            return new GetUserProjectsResponse
            {
                Success = true,
                Message = "User projects retrieved successfully",
                UserProjectTiles = (List<ProjectTileDto>)projectTileDtos,
            };
        }
    }
}
