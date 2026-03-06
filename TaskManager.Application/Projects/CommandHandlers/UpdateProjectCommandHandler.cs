using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using TaskManager.Application.Common;
using TaskManager.Application.Projects.Commands;
using TaskManager.Application.Projects.DTOs;
using TaskManager.Domain.Common;
using TaskManager.Domain.Entities;
using TaskManager.Domain.Interfaces;

namespace TaskManager.Application.Projects.CommandHandlers
{
    public class UpdateProjectCommandHandler(IUnitOfWork unitOfWork, UserManager<User> userManager, IDistributedCache cache, ILogger<UpdateProjectCommandHandler> logger) : IRequestHandler<UpdateProjectCommand, Result<ProjectDetailsDto>>
    {
        private readonly IUnitOfWork _unitOfWork = unitOfWork;
        private readonly UserManager<User> _userManager = userManager;
        private readonly IDistributedCache _cache = cache; 
        private readonly ILogger<UpdateProjectCommandHandler> _logger = logger; 
        public async Task<Result<ProjectDetailsDto>> Handle(UpdateProjectCommand request, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Beginning Command Handler"); 
            if (request is null || request.UserId == Guid.Empty || request.ProjectId == Guid.Empty)
                return Result<ProjectDetailsDto>.Failure("Invalid request.");

            var user = await _userManager.FindByIdAsync(request.UserId.ToString());
            if (user is null)
                return Result<ProjectDetailsDto>.Failure("User not found.");

            var project = await _unitOfWork.ProjectRepository.GetProjectWithoutTasksAsync(request.ProjectId, cancellationToken);
            if (project is null)
                return Result<ProjectDetailsDto>.Failure("Project not found.");

            if (project.OwnerId != request.UserId)
                return Result<ProjectDetailsDto>.Failure("Unauthorized: You do not have permission to update this project.");

            if(request.NewTitle is not null && request.NewTitle != project.Title)
            {
                var updateTitleResult = project.UpdateTitle(request.NewTitle); 
                if (updateTitleResult.IsFailure)
                    return Result<ProjectDetailsDto>.Failure(updateTitleResult.ErrorMessage ?? "Failed to update project Title.");
            }

            if (request.NewDescription is not null && request.NewDescription != project.Description)
            {
                var updateDescriptionResult = project.UpdateDescription(request.NewDescription);
                if (updateDescriptionResult.IsFailure)
                    return Result<ProjectDetailsDto>.Failure(updateDescriptionResult.ErrorMessage ?? "Failed to update project description.");
            }

            //Handling assignee keys here because they may not be accessible from Presentation
            //at the time of project completion (to pass to CacheInvalidator)
            var assigneeIds = await _unitOfWork.ProjectRepository.GetProjectTodoItemAssigneeIds(request.ProjectId, cancellationToken);


            try
            {
                _logger.LogInformation("Saving Updated Project");
                _unitOfWork.ProjectRepository.Update(project);
                await _unitOfWork.SaveChangesAsync(cancellationToken);
                
                var response = new ProjectDetailsDto
                {
                    Id = project.Id,
                    Title = project.Title,
                    Description = project.Description,
                    CreatedOn = project.CreatedOn
                };

                if (assigneeIds.Count > 0)
                {
                    _logger.LogInformation("More than 1 AssigneeKeys found");

                    foreach (var key in assigneeIds)
                    {
                        _logger.LogInformation("Removing: " + key);
                        await _cache.RemoveAsync(CacheKeys.AssignedTodoItems(key), cancellationToken);
                    }
                }

                return Result<ProjectDetailsDto>.Success(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Issue Updatig Project");
                return Result<ProjectDetailsDto>.Failure($"An error occurred while updating the project description.");
            }
        }
    }
}
