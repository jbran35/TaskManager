using MediatR;
using Microsoft.Extensions.Logging;
using TaskManager.Application.Common;
using TaskManager.Application.TodoItems.DTOs;
using TaskManager.Application.TodoItems.Queries;
using TaskManager.Domain.Common;
using TaskManager.Domain.Interfaces;

namespace TaskManager.Application.TodoItems.QueryHandlers
{
    public class GetTodoItemDetailedViewQueryHandler(IUnitOfWork unitOfWork, ILogger<GetTodoItemDetailedViewQueryHandler> logger) : IRequestHandler<GetTodoItemDetailedViewQuery, Result<TodoItemEntry>>
    {
        private readonly IUnitOfWork _unitOfWork = unitOfWork;
        private readonly ILogger<GetTodoItemDetailedViewQueryHandler> _logger = logger;
        public async Task<Result<TodoItemEntry>> Handle(GetTodoItemDetailedViewQuery request, CancellationToken cancellationToken)
        {
            //Validate Request
            if (request is null || request.Id == Guid.Empty || request.UserId == Guid.Empty)
                return Result<TodoItemEntry>.Failure("Invalid Request");



            //Validate TodoItem
            var todoItem = await _unitOfWork.TodoItemRepository.GetTodoItemByIdAsync(request.Id, cancellationToken);

            if (todoItem is null || todoItem.OwnerId != request.UserId || todoItem.Project.OwnerId != request.UserId)
                return Result<TodoItemEntry>.Failure("Task Not Found");

            //Map to DTO & Return
            var TodoItemEntry = new TodoItemEntry
            {
                Id = todoItem.Id,
                Title = todoItem.Title.Value,
                Description = todoItem.Description.Value,
                ProjectTitle = todoItem.Project.Title,
                AssigneeName = todoItem.Assignee?.FullName ?? string.Empty,
                OwnerName = todoItem.Owner?.FullName ?? string.Empty,
                Priority = todoItem.Priority ?? Domain.Enums.Priority.None,
                DueDate = todoItem.DueDate,
                CreatedOn = todoItem.CreatedOn,
                Status = todoItem.Status
            };
                
            return Result<TodoItemEntry>.Success(TodoItemEntry);
        }
    }
}
