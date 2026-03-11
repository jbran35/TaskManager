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

            var user = await _userManager.FindByIdAsync(request.UserId.ToString());
            if (user is null)
                return Result<CompleteProjectResponse>.Failure("User not found.");

            var project = await _unitOfWork.ProjectRepository.GetProjectWithTasksAsync(request.ProjectId, cancellationToken);
            if(project is null)
                return Result<CompleteProjectResponse>.Failure("Project not found.");

            if(request.UserId != project.OwnerId)
                return Result<CompleteProjectResponse>.Failure("Unauthorized.");

            //Handling assignee keys here because they may not be accessible from Presentation
            //at the time of project completion (to pass to CacheInvalidator)
            var assigneeIds = await _unitOfWork.ProjectRepository.GetProjectTodoItemAssigneeIds(request.ProjectId, cancellationToken);

            _logger.LogInformation("Marking Complete");
            //var result = project.MarkAsComplete();


            //if(result.IsFailure)
            //{
            //    _logger.LogInformation("Failed To Mark Tasks As Complete");
            //    return Result<CompleteProjectResponse>.Failure(result.ErrorMessage!);
            //}

            try
            {
                await _unitOfWork.ProjectRepository.CompleteProjectTodoItems(project.Id, cancellationToken);

                _logger.LogInformation("Saving Changes");
                _unitOfWork.ProjectRepository.Update(project);
                await _unitOfWork.SaveChangesAsync(cancellationToken);

                var newProjectTile = new ProjectTileDto
                {
                    Id = project.Id,
                    Title = project.Title,
                    TotalTodoItemCount = project.TodoItems.Count,
                    CompleteTodoItemCount = project.TodoItems.Count,
                    CreatedOn = project.CreatedOn,
                    Status = Domain.Enums.Status.Complete
                }; 

                var response = new CompleteProjectResponse(newProjectTile);

                if (assigneeIds.Count > 0)
                {
                    _logger.LogInformation("More than 1 AssigneeKeys found");

                    foreach (var key in assigneeIds)
                    {
                        _logger.LogInformation("Removing: " + key);
                        await _cache.RemoveAsync(CacheKeys.AssignedTodoItems(key), CancellationToken.None);
                    }
                }

                //await _cache.RemoveAsync(CacheKeys.ProjectTiles(user.Id), CancellationToken.None); 

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
