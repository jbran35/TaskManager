using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using TaskManager.Application.Common;
using TaskManager.Application.Projects.Commands;
using TaskManager.Application.Projects.DTOs.Responses;
using TaskManager.Domain.Common;
using TaskManager.Domain.Entities;
using TaskManager.Domain.Interfaces;

namespace TaskManager.Application.Projects.CommandHandlers
{
    public class DeleteProjectCommandHandler(IUnitOfWork unitOfWork, UserManager<User> userManager, IDistributedCache cache, ILogger<DeleteProjectCommandHandler> logger) : IRequestHandler<DeleteProjectCommand, Result<DeleteProjectResponse>>
    {
        private readonly IUnitOfWork _unitOfWork = unitOfWork; 
        private readonly UserManager<User> _userManager = userManager;
        private readonly IDistributedCache _cache = cache;
        private readonly ILogger<DeleteProjectCommandHandler> _logger = logger; 
        public async Task<Result<DeleteProjectResponse>> Handle(DeleteProjectCommand request, CancellationToken cancellationToken)
        {
            //Validate the request
            if (request is null || request.UserId == Guid.Empty || request.ProjectId == Guid.Empty)
                return Result<DeleteProjectResponse>.Failure("Invalid request.");

            //Check if the user exists
            var user = await _userManager.FindByIdAsync(request.UserId.ToString());
            if (user is null)
                return Result<DeleteProjectResponse>.Failure("User not found.");

            //Check if the project exists and if the user is the owner
            var project = await _unitOfWork.ProjectRepository.GetProjectWithoutTasksAsync(request.ProjectId, cancellationToken);

            if (project is null || project.OwnerId != request.UserId)
                return Result<DeleteProjectResponse>.Failure("Project not found or user is not the owner.");


            //Delete project and save changes
            try
            {
                _logger.LogInformation("Removing Project From Repo");
                _unitOfWork.ProjectRepository.Delete(project);
                _logger.LogInformation("Saving Changes");
                await _unitOfWork.SaveChangesAsync(cancellationToken);


                _logger.LogInformation("Removing Project From Redis");

                string detailsKey = CacheKeys.ProjectDetailedViews(user.Id, project.Id);
                string tilesKey = CacheKeys.ProjectTiles(user.Id);

                await _cache.RemoveAsync(detailsKey, cancellationToken);
                await _cache.RemoveAsync(tilesKey, cancellationToken);

                _logger.LogInformation("Project Removed From Redis Successfully");
                var response = new DeleteProjectResponse(project.Id, "Project Successfully Deleted");

                return Result<DeleteProjectResponse>.Success(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Issue Removing Project");

                return Result<DeleteProjectResponse>.Failure($"An error occurred while deleting the project.");
            }

        }
    }
}
