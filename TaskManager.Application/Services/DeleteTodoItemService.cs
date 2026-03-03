using Microsoft.AspNetCore.Identity;
using TaskManager.Application.DTOs;
using TaskManager.Application.Interfaces;
using TaskManager.Domain.Entities;

namespace TaskManager.Application.Services
{
    public class DeleteTodoItemService : IDeleteTodoItemService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly UserManager<User> _userManager;
        public DeleteTodoItemService(IUnitOfWork unitOfWork, UserManager<User> userManager)
        {
            _unitOfWork = unitOfWork;
            _userManager = userManager;
        }
        public async Task<DeleteTodoItemResponse> DeleteTodoItemAsync(Guid todoItemId, Guid userId)
        {
            //Validate input parameters
            if(todoItemId == Guid.Empty || userId == Guid.Empty)
            {
                return new DeleteTodoItemResponse
                {
                    Success = false,
                    Message = "Invalid TodoItem or User ID."
                };
            }

            //Verify that User exissts
            var user = await _userManager.FindByIdAsync(userId.ToString());
            if (user is null)
            {
                return new DeleteTodoItemResponse
                {
                    Success = false,
                    Message = "User ID Not Found."
                };
            }

            // Load task from repository
            var task = await _unitOfWork.TodoItemRepository.GetTodoItemByIdAsync(todoItemId);
            if (task is null)
            {
                return new DeleteTodoItemResponse
                {
                    Success = false,
                    Message = "Task Not Found."
                };
            }

            // Check that user is owner of task project
            var projectOwnerId = await _unitOfWork.ProjectRepository.GetProjectOwnerIdAsync(task.ProjectId);
            if (projectOwnerId != userId)
            {
                return new DeleteTodoItemResponse
                {
                    Success = false,
                    Message = "You are not authorized to delete this task."
                };
            }
            
            // Delete the task
            try
            {
                Console.WriteLine("Deleting task in application service");
                _unitOfWork.TodoItemRepository.Delete(todoItemId);
                await _unitOfWork.SaveChangesAsync();
            }

            catch (Exception ex)
            {
                _unitOfWork.Dispose(); 
                return new DeleteTodoItemResponse
                {
                    Success = false,
                    Message = "Could not delete this task: \n" + ex.Message
                };
            }

            return new DeleteTodoItemResponse
            {
                TodoItemId = task.Id,
                TodoItemName = task.Name.Value,
                Success = true,
                Message = "Task deleted successfully."
            };
        }
    }
}
