using Microsoft.AspNetCore.Identity;
using TaskManager.Application.DTOs;
using TaskManager.Application.Interfaces;
using TaskManager.Domain.Entities;


namespace TaskManager.Application.Services
{
    public class CompleteTodoItemService : ICompleteTodoItemService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly UserManager<User> _userManager;

        public CompleteTodoItemService(IUnitOfWork unitOfWork, UserManager<User> userManager)
        {
            _unitOfWork = unitOfWork;
            _userManager = userManager;
        }
        public async Task<CompleteTodoItemResponse> CompleteTaskAsync(Guid todoItemId, Guid userId)
        {
            bool markedComplete = false;
            bool markedIncomplete = false;

            // Validate input
            if (userId == Guid.Empty || todoItemId == Guid.Empty)
            {
                return new CompleteTodoItemResponse
                {
                    Success = false,
                    Message = "Request invalid"
                };
            }

            // Ensure User exists
            var user = await _userManager.FindByIdAsync(userId.ToString());
            if (user is null)
            {
                return new CompleteTodoItemResponse
                {
                    Success = false,
                    Message = "User not found"
                };
            }

            //Ensure Task Exists
            var todoItem = await _unitOfWork.TodoItemRepository.GetTodoItemByIdAsync(todoItemId);

            if (todoItem is null)
            {
                return new CompleteTodoItemResponse
                {
                    Success = false,
                    Message = "Task not found"
                };
            }

            //Verify Project Exists
            var project = await _unitOfWork.ProjectRepository.GetProjectByIdAsync(todoItem.ProjectId);

            if (project is null)
            {
                return new CompleteTodoItemResponse
                {
                    Success = false,
                    Message = "Associated project not found"
                };
            }

            //Ensure User is Task Owner or Assignee
            if (project.OwnerId != userId && todoItem.AssigneeId != userId)
            {
                return new CompleteTodoItemResponse
                {
                    Success = false,
                    Message = "User not authorized to complete this task"
                };
            }

            //Mark Task Complete
            if(todoItem.Status == Domain.Enums.Status.Complete)
            {
                todoItem.MarkAsIncomplete();
                markedIncomplete = true;
            }

            else
            {
                todoItem.MarkAsComplete();
                markedComplete = true;
            }

            //Update Task
            try
            {
                _unitOfWork.TodoItemRepository.Update(todoItem);
                await _unitOfWork.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                return new CompleteTodoItemResponse
                {
                    Success = false,
                    Message = "Problem updating task status: \n" + ex.Message
                };
            }

            //Return response
            return new CompleteTodoItemResponse
            {
                MarkedComplete = markedComplete,
                MarkedIncomplete = markedIncomplete,
                Success = true,
                Message = "Task Marked Complete"
            };
        }
    }
}
