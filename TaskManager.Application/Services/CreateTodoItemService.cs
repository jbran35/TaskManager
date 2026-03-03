using Microsoft.AspNetCore.Identity;
using TaskManager.Application.DTOs;
using TaskManager.Application.Interfaces;
using TaskManager.Domain.Entities;
using TaskManager.Domain.Enums;
using TaskManager.Domain.Interfaces;

namespace TaskManager.Application.Services
{
    public class CreateTodoItemService : ICreateTodoItemService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly UserManager<User> _userManager;

        public CreateTodoItemService(IUnitOfWork unitOfWork, UserManager<User> userManager)
        {
            _unitOfWork = unitOfWork;
            _userManager = userManager;
        }
        public async Task<CreateTodoItemResponse> CreateTodoItemAsync(CreateTodoItemRequest request, Guid userId)
        {
            //Validate input
            if(userId == Guid.Empty || request is null)
            {
                return new CreateTodoItemResponse
                {
                    Success = false,
                    Message = "Invalid input."
                };
            }


            //Verify that user exists
            var user = await _userManager.FindByIdAsync(userId.ToString());
            if (user is null)
            {
                return new CreateTodoItemResponse
                {
                    Success = false,
                    Message = "UserId Not Found."
                };
            }

            // Check that project is in repo
            Console.WriteLine("Checking that project in repo");

            var project = await _unitOfWork.ProjectRepository.GetProjectWithoutTasksAsync(request.ProjectId);
            if (project is null)
            {
                return new CreateTodoItemResponse
                {
                    Success = false,
                    Message = "Project Not Found."
                };
            }

            // Check that user is owner of project
            if (project.OwnerId != userId)
            {
                return new CreateTodoItemResponse
                {
                    Success = false,
                    Message = "User is not the owner of the project."
                };
            }

            //Check that assignee (if applicable) is in repo
            User? assignee = null;
            if (request.AssigneeId.HasValue && request.AssigneeId != null && request.AssigneeId != Guid.Empty)
            {
                assignee = await _userManager.FindByIdAsync(request.AssigneeId.Value.ToString());
                if (assignee is null)
                {
                    return new CreateTodoItemResponse
                    {
                        Success = false,
                        Message = "Assignee Not Found."
                    };
                }
            }

            // Create Task
            var task = new TodoItem(request.Name, project.Id,
                request.Priority ?? Priority.None, request.Description ?? string.Empty,
                (request.AssigneeId == Guid.Empty ? null : request.AssigneeId), request.DueDate ?? null, userId);

            // Save Task to Repo
            try
            {
                Console.WriteLine("Pushing unit of work changes in CreateTodoItemService");
                _unitOfWork.TodoItemRepository.Add(task);
                var result = await _unitOfWork.SaveChangesAsync(); // Commit the transaction
                Console.WriteLine("Completed push. Result: " + result);
            }
            catch (Exception ex)
            {
                return new CreateTodoItemResponse
                {
                    Success = false,
                    Message = "Project could not be created: \n" + ex.Message
                };
            }

            // Create TaskListEntryDto
            var taskListEntryDto = new TodoItemListEntryDto
            {
                // ProjectId, ProjectName, TaskId, Name, AssigneeId, AssigneeName, DueDate, Status, Priority
                ProjectId = task.ProjectId,
                ProjectName = project.Name.Value,
                TodoItemId = task.Id,
                Title = task.Name.Value,
                AssigneeId = task.AssigneeId,
                AssigneeName = task.Assignee?.FullName ?? string.Empty,
                DueDate = task.DueDate,
                Status = task.Status,
                Priority = task.Priority,
            };

            // Return Response
            return new CreateTodoItemResponse
            {
                TodoItemListEntry = taskListEntryDto,
                Success = true,
                Message = "Task Created Successfully."
            };
        }
    }
}
