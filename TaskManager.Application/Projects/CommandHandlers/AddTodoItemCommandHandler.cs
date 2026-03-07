using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using TaskManager.Application.Projects.Commands;
using TaskManager.Application.Projects.Events;
using TaskManager.Application.TodoItems.DTOs;
using TaskManager.Domain.Common;
using TaskManager.Domain.Entities;
using TaskManager.Domain.Interfaces;
using TaskManager.Domain.ValueObjects;

namespace TaskManager.Application.Projects.CommandHandlers
{
    public class AddTodoItemCommandHandler(IUnitOfWork unitOfWork, UserManager<User> userManager,
        ILogger<AddTodoItemCommandHandler> logger, IMediator mediator) : IRequestHandler<AddTodoItemCommand, Result<TodoItemEntry>>
    {
        private readonly IUnitOfWork _unitOfWork = unitOfWork;
        private readonly UserManager<User> _userManager = userManager;
        private readonly ILogger<AddTodoItemCommandHandler> _logger = logger;
        private readonly IMediator _mediator = mediator; 
        public async Task<Result<TodoItemEntry>> Handle(AddTodoItemCommand request, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Handling Command"); 
          
            var user = await _userManager.FindByIdAsync(request.UserId.ToString());
            if (user is null)
                return Result<TodoItemEntry>.Failure("User not found.");

            var project = await _unitOfWork.ProjectRepository.GetProjectWithoutTasksAsync(request.ProjectId, cancellationToken);
            if (project is null)
                return Result<TodoItemEntry>.Failure("Project not found.");

            if(!project.OwnerId.Equals(request.UserId))
                return Result<TodoItemEntry>.Failure("Unauthorized.");

            Guid assigneeId = Guid.Empty;
            bool hasAssigneeId = request.AssigneeId != Guid.Empty && request.AssigneeId is not null;
            bool assigneeIsValidated = false;

            if (hasAssigneeId)
            {
                assigneeId = request.AssigneeId!.Value;
                User? assignee = null;
                assignee = await _userManager.FindByIdAsync(assigneeId.ToString());

                if (assignee is null)
                    return Result<TodoItemEntry>.Failure("Assignee Could Not Be Found.");

                assigneeIsValidated = true;
            }

            var todoItemTitleResult = Title.Create(request.Title);
            if (todoItemTitleResult.IsFailure)
                return Result<TodoItemEntry>.Failure(todoItemTitleResult.ErrorMessage ?? "Invalid project title.");

            var todoItemDescriptionResult = Description.Create(request.Description!);
            if (todoItemDescriptionResult.IsFailure)
                return Result<TodoItemEntry>.Failure(todoItemDescriptionResult.ErrorMessage ?? "Invalid project description.");

            var todoItemResult = TodoItem.Create(todoItemTitleResult.Value, todoItemDescriptionResult.Value, request.UserId, request.ProjectId, request.AssigneeId,
                request.Priority, request.DueDate);

            if (todoItemResult.IsFailure)
                return Result<TodoItemEntry>.Failure(todoItemResult.ErrorMessage ?? "Failed to create todo item.");

            var todoItem = todoItemResult.Value;

            project.AddTodoItem(todoItem);
            project.MarkAsIncomplete();
            _unitOfWork.TodoItemRepository.Add(todoItem);

            try
            {
                _logger.LogInformation("Saving Changes");
                await _unitOfWork.SaveChangesAsync(cancellationToken);

                var listEntryDto = new TodoItemEntry
                {
                    Id = todoItem.Id,
                    AssigneeId = todoItem.AssigneeId, 
                    OwnerId = todoItem.OwnerId,
                    Title = todoItem.Title,
                    Description = todoItem.Description,
                    ProjectTitle = todoItem.Project.Title,
                    AssigneeName = todoItem.Assignee?.FullName ?? string.Empty,
                    OwnerName = todoItem.Owner?.FullName ?? string.Empty,
                    Priority = todoItem.Priority ?? Domain.Enums.Priority.None,
                    DueDate = todoItem.DueDate,
                    CreatedOn = todoItem.CreatedOn,
                    Status = todoItem.Status
                };

                if (assigneeIsValidated)
                {
                    var assignedTodoItemCreatedEvent = new AssignedTodoItemCreatedEvent(assigneeId);
                    await _mediator.Publish(assignedTodoItemCreatedEvent, cancellationToken);
                }

                return Result<TodoItemEntry>.Success(listEntryDto);
            }

            catch (Exception ex)
            {
                _logger.LogError(ex, "Issue Encountered"); 
                return Result<TodoItemEntry>.Failure($"An error occurred while adding the todo item.");
            }
        }
    }
}
