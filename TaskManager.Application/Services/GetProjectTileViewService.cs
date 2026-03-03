using Microsoft.AspNetCore.Identity;
using TaskManager.Application.DTOs;
using TaskManager.Application.Interfaces;
using TaskManager.Domain.Entities;

namespace TaskManager.Application.Services
{
    public class GetProjectTileViewService : IGetProjectTileViewService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly UserManager<User> _userManager;

        public GetProjectTileViewService(IUnitOfWork unitOfWork, UserManager<User> userManager)
        {
            _unitOfWork = unitOfWork;
            _userManager = userManager;
        }

        public async Task<GetProjectTileViewResponse> GetProjectTileViewAsync(Guid projectId, Guid userId)
        {

            // Validate input 

            if (projectId == Guid.Empty)
            {
                return new GetProjectTileViewResponse
                {
                    Success = false,
                    Message = "Project ID is required."
                };
            }

            if (userId == Guid.Empty)
            {
                return new GetProjectTileViewResponse
                {
                    Success = false,
                    Message = "User ID is required."
                };
            }

            //Retrieve and validate Project
            var project = await _unitOfWork.ProjectRepository.GetProjectByIdAsync(projectId);
            
            if (project is null)
            {
                return new GetProjectTileViewResponse
                {
                    Success = false,
                    Message = "Project not found."
                };
            }

            // Ensure that the user is the project owner
            var user = await _userManager.FindByIdAsync(userId.ToString());
            if (user is null)
            {
                return new GetProjectTileViewResponse
                {
                    Success = false,
                    Message = "User not found."
                };
            }

            if (project.OwnerId != user.Id)
            {
                return new GetProjectTileViewResponse
                {
                    Success = false,
                    Message = "You do not have permission to view this project."
                };
            }

            // Retrieve todo items for the project and calculate task count
            var todoItems = await _unitOfWork.TodoItemRepository.GetTodoItemsByProjectIdAsync(project.Id);
            int totalTodoItems = 0;
            int completeTodoItems = 0;
            double completePercentage = 0.00; 

            if (todoItems is not null && todoItems.Count() > 0)
            {
                totalTodoItems = todoItems.Count();
                completeTodoItems = todoItems.Where(t => t.Status == Domain.Enums.Status.Complete).ToList().Count();
                completePercentage = completeTodoItems / totalTodoItems; 
            }

        

            return new GetProjectTileViewResponse
            {
                //ProjectId, ProjectName, Status, TotalTodoItemsCount, CompletedTodoItemsCount, CompletePercentage
                ProjectTile = new ProjectTileDto
                {
                    ProjectId = project.Id,
                    ProjectName = project.Name.Value,
                    TotalTodoItems = totalTodoItems,
                    CompletedTodoItems = completeTodoItems,
                    CompletePercentage = completePercentage,
                    Status = project.Status,
                },

                Success = true,
                Message = "Project tile view retrieved successfully."
            };  
        }
    }
}
