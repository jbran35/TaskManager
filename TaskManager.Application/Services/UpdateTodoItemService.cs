using Microsoft.AspNetCore.Identity;
using TaskManager.Application.DTOs;
using TaskManager.Application.Interfaces;
using TaskManager.Domain.Entities;

namespace TaskManager.Application.Services
{
    public class UpdateTodoItemService (IUnitOfWork unitOfWork, UserManager<User> userManager) : IUpdateTodoItemService
    {
        private readonly IUnitOfWork _unitOfWork = unitOfWork;
        private readonly UserManager<User> _userManager = userManager;

        public async Task<UpdateTodoItemResponse> UpdateTodoItemAsync(UpdateTodoItemRequest request, Guid userId, Guid todoItemId)
        {
            //Verify request & input
            if (userId == Guid.Empty || request.ProjectId == Guid.Empty || request.Title is null)
            {
                return new UpdateTodoItemResponse
                {
                    Success = false,
                    Message = "Invalid request."
                };
            }

            //Check if user exists
            var user = await _userManager.FindByIdAsync(userId.ToString());
            if (user is null)
            {
                return new UpdateTodoItemResponse
                {
                    Success = false,
                    Message = "User not found."
                };
            }

            //Check if current project exists
            var project = await _unitOfWork.ProjectRepository.GetProjectByIdAsync(request.ProjectId);
            if (project is null)
            {
                return new UpdateTodoItemResponse
                {
                    Success = false,
                    Message = "Project not found."
                };
            }

            //Check if user is the owner of the task's project
            if (project.OwnerId != userId)
            {
                return new UpdateTodoItemResponse
                {
                    Success = false,
                    Message = "User must be task-project owner"
                };
            }

            // Check if task exists
            var todoItem = await _unitOfWork.TodoItemRepository.GetTodoItemByIdAsync(todoItemId);
            
            var assigneFirstName = string.Empty;
            var assigneeLastName = string.Empty;

            //Check that assignee exists
            if (request.AssigneeId != null && request.AssigneeId != Guid.Empty)
            {
                var assignee = await _userManager.FindByIdAsync(request.AssigneeId.Value.ToString());
                
                if (assignee != null && assignee.FirstName != null && assignee.LastName != null)
                {
                    assigneFirstName = assignee.FirstName; 
                    assigneeLastName = assignee.LastName;
                }
            }

            if (todoItem is null)
            {
                return new UpdateTodoItemResponse
                {
                    Success = false,
                    Message = "Task not found."
                };
            }

            // Update task
            try
            {
                if (request.Title != null)
                {
                    todoItem.UpdateName(request.Title);
                }

                if (request.Description != null)
                {
                    todoItem.UpdateDescription(request.Description);
                }

                if (request.AssigneeId != null)
                {
                    todoItem.AssignToUser(request.AssigneeId.Value);
                }

                if (request.Status != null)
                {
                    todoItem.UpdateStatus(request.Status ?? Domain.Enums.Status.Incomplete);
                }

                if (request.Priority != null)
                {
                    todoItem.UpdatePriority(request.Priority ?? Domain.Enums.Priority.None);
                }

                if (request.DueDate != null)
                {
                    todoItem.UpdateDueDate(request.DueDate ?? null);
                }

                _unitOfWork.TodoItemRepository.Update(todoItem);
                await _unitOfWork.SaveChangesAsync();
            }

            catch (Exception ex)
            {
                return new UpdateTodoItemResponse
                {
                    Success = false,
                    Message = "Unable to update task: + \n" + ex.Message
                };

            }

            var taskListDto = new TodoItemListEntryDto
            {
                ProjectId = todoItem.ProjectId,
                ProjectName = project.Name.Value,
                TodoItemId = todoItem.Id,
                Title = todoItem.Name.Value,
                AssigneeId = todoItem.AssigneeId,
                AssigneeName = todoItem.Assignee?.FullName ?? string.Empty,
                DueDate = todoItem.DueDate ?? null,
                Status = todoItem.Status,
                Priority = todoItem.Priority,
            };

            return new UpdateTodoItemResponse
            {
                UpdatedTodoItem = taskListDto,
                Success = true,
                Message = "Task updated successfully."
            };
        }
    }
}
