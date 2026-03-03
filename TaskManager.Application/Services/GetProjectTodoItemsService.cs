using Microsoft.AspNetCore.Identity;
using TaskManager.Application.DTOs;
using TaskManager.Application.Interfaces;
using TaskManager.Domain.Entities;

namespace TaskManager.Application.Services
{
    public class GetProjectTodoItemsService : IGetProjectTodoItemsService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly UserManager<User> _userManager;

        public GetProjectTodoItemsService(IUnitOfWork unitOfWork, UserManager<User> userManager)
        {
            _unitOfWork = unitOfWork;
            _userManager = userManager;
        }

        public async Task<GetProjectTodoItemsResponse> GetProjectTodoItemsAsync(Guid projectId, Guid userId)
        {
            //Validate UserID 
            if (userId == Guid.Empty)
            {
                return new GetProjectTodoItemsResponse
                {
                    Success = false,
                    Message = "Invalid UserId"
                };
            }

            //Validate ProjectId 
            if (projectId == Guid.Empty)
            {
                return new GetProjectTodoItemsResponse
                {
                    Success = false,
                    Message = "Invalid ProjectId"
                };
            }

            //Ensure User Exists
            var user = await _userManager.FindByIdAsync(userId.ToString());

            if (user is null)
            {
                return new GetProjectTodoItemsResponse
                {
                    Success = false,
                    Message = "User Not Found"
                };
            }

            //Ensure Project Exists
            var project = await _unitOfWork.ProjectRepository.GetProjectByIdAsync(projectId);

            if (project is null)
            {
                return new GetProjectTodoItemsResponse
                {
                    Success = false,
                    Message = "Project Not Found"
                };
            }

            //Ensure User is Project Owner
            if (project.OwnerId != userId)
            {
                return new GetProjectTodoItemsResponse
                {
                    Success = false,
                    Message = "User is not authorized to view tasks for this project"
                };
            }

            //Get TodoItems for Project and ensure they exist. 
            var todoItems = await _unitOfWork.TodoItemRepository.GetTodoItemsByProjectIdAsync(projectId);

            if (todoItems is null)
            {
                return new GetProjectTodoItemsResponse
                {
                    Success = true,
                    Message = "Project Found, No Tasks Assigned"
                };
            }

            Console.WriteLine("In GetProjTask service");


            //Map TodoItems to TodoItemListEntryDtos
            var todoItemListEntryDtos = todoItems.Select(t => new TodoItemListEntryDto
            {
                ProjectId = t.ProjectId,
                ProjectName = t.Project.Name.Value,
                TodoItemId = t.Id,
                Title = t.Name.Value,
                AssigneeId = t.AssigneeId,
                AssigneeName = t.Assignee?.FullName ?? string.Empty,
                DueDate = t.DueDate,
                Status = t.Status,
                Priority = t.Priority,
            }).ToList();

            foreach(var todo in todoItemListEntryDtos)
            {
                Console.WriteLine("Pulled tasks' assignees:" + todo.AssigneeName);
            }

            return new GetProjectTodoItemsResponse
            {
                TodoItems = todoItemListEntryDtos,
                Success = true,
                Message = "Tasks retrieved successfully"
            };
        }
    }
}
