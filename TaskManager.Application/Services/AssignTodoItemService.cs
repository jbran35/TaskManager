using Microsoft.AspNetCore.Identity;
using TaskManager.Application.DTOs;
using TaskManager.Application.Interfaces;
using TaskManager.Domain.Entities;

namespace TaskManager.Application.Services
{
    public class AssignTodoItemService : IAssignTodoItemService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly UserManager<User> _userManager;

        public AssignTodoItemService(IUnitOfWork unitOfWork, UserManager<User> userManager)
        {
            _unitOfWork = unitOfWork;
            _userManager = userManager;
        }

        public async Task<AssignTodoItemResponse> AssignTodoItemAsync(Guid userId, Guid assigneeId, Guid todoItemId, Guid projectId)
        {
            //Check If user Exists
            var project = await _unitOfWork.ProjectRepository.GetProjectByIdAsync(projectId);

            //Check that user is owner of the todo item's project

            if (project is null || project.OwnerId != userId)
            {
                return new AssignTodoItemResponse
                {
                    Success = false,
                    Message = "User is not the owner of the project."
                };
            }

            //Check that task exists and is a part of the specified project
            var todoItem = await _unitOfWork.TodoItemRepository.GetTodoItemByIdAsync(todoItemId);

            if (todoItem is null || todoItem.ProjectId != projectId)
            {
                return new AssignTodoItemResponse
                {
                    Success = false,
                    Message = "Todo item not found in the specified project."
                };
            }

            //Check that assignee exists
            var assignee = await _userManager.FindByIdAsync(assigneeId.ToString());
            if (assignee is null)
            {
                return new AssignTodoItemResponse
                {
                    Success = false,
                    Message = "Assignee not found."
                };
            }

            //Assign the task
            try
            {
                todoItem.AssignToUser(assigneeId);
                //todoItem.OwnerId = userId; 

                _unitOfWork.TodoItemRepository.Update(todoItem);

                await _unitOfWork.SaveChangesAsync();

                return new AssignTodoItemResponse
                {
                    Success = true,
                    Message = "Todo item assigned successfully."
                };
            }
            catch (Exception ex)
            {
                return new AssignTodoItemResponse
                {
                    Success = false,
                    Message = $"An error occurred while assigning the todo item: {ex.Message}"
                };
            }

        }
    }
}
