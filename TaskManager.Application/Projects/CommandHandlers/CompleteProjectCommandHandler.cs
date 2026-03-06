using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using TaskManager.Application.Common;
using TaskManager.Application.Projects.Commands;
using TaskManager.Application.Projects.DTOs;
using TaskManager.Application.Projects.DTOs.Responses;
using TaskManager.Domain.Common;
using TaskManager.Domain.Entities;
using TaskManager.Domain.Interfaces;

namespace TaskManager.Application.Projects.CommandHandlers
{
    public class CompleteProjectCommandHandler(IUnitOfWork unitOfWork, UserManager<User> userManager, 
        IDistributedCache cache, ILogger<CompleteProjectCommandHandler> logger) : IRequestHandler<CompleteProjectCommand, Result<CompleteProjectResponse>>
    {
        private readonly IUnitOfWork _unitOfWork = unitOfWork;
        private readonly UserManager<User> _userManager = userManager;
        private readonly ILogger<CompleteProjectCommandHandler> _logger = logger;
        private readonly IDistributedCache _cache = cache; 
        public async Task<Result<CompleteProjectResponse>> Handle(CompleteProjectCommand request, CancellationToken cancellationToken)
        {
            _logger.LogInformation("In Handler");
            if (request is null || request.UserId == Guid.Empty || request.ProjectId == Guid.Empty)
                return Result<CompleteProjectResponse>.Failure("Invalid request.");

            var user = await _userManager.FindByIdAsync(request.UserId.ToString());
            if (user is null)
                return Result<CompleteProjectResponse>.Failure("User not found.");

            var project = await _unitOfWork.ProjectRepository.GetProjectWithTasksAsync(request.ProjectId, cancellationToken);

            if(project is null)
                return Result<CompleteProjectResponse>.Failure("Project not found.");

            if(request.UserId != project.OwnerId)
                return Result<CompleteProjectResponse>.Failure("Unauthorized.");

            var assigneeIds = project.TodoItems
                .Where(t => t.AssigneeId is not null && t.AssigneeId != Guid.Empty)
                .Select(t => t.AssigneeId)
                .Distinct()
                .ToArray();

            _logger.LogInformation("Marking Complete");
            var result = project.MarkAsComplete();

             if(result.IsFailure)
                return Result<CompleteProjectResponse>.Failure(result.ErrorMessage!);

            try
            {
                _logger.LogInformation("Saving Changes");
                await _unitOfWork.SaveChangesAsync(cancellationToken);

                var newProjectTile = new ProjectTileDto
                {
                    Id = project.Id,
                    Title = project.Title,
                    TotalTodoItemCount = project.TodoItems.Count,
                    CompleteTodoItemCount = project.TodoItems.Where(t => t.Status == Domain.Enums.Status.Complete).Count(),
                    CreatedOn = project.CreatedOn,
                    Status = project.Status
                };

                var response = new CompleteProjectResponse(newProjectTile);

                var keys = new List<string>();
                keys.Add(CacheKeys.ProjectTiles(user.Id));
                keys.Add(CacheKeys.ProjectDetailedViews(user.Id, project.Id)); 

                foreach(var id in assigneeIds)
                {
                    keys.Add(CacheKeys.AssignedTodoItems(id ?? Guid.Empty)); 
                }

                foreach (var key in keys)
                {
                    _logger.LogInformation("Removing with key: " + key);
                    await _cache.RemoveAsync(key); 
                }

                _logger.LogInformation("Returning");
                return Result<CompleteProjectResponse>.Success(response);

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Issue Completing Project");
                return Result<CompleteProjectResponse>.Failure("An error occurred while completing the project.");
            }
        }
    }
}
