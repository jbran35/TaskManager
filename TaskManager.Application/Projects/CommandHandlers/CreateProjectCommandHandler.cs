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
using TaskManager.Domain.ValueObjects;

namespace TaskManager.Application.Projects.CommandHandlers
{
    public class CreateProjectCommandHandler(
        IUnitOfWork unitOfWork, UserManager<User> userManager, IDistributedCache cache, 
        ILogger<CreateProjectCommandHandler> logger) : IRequestHandler<CreateProjectCommand, Result<ProjectTileDto>>
    {
        private readonly IUnitOfWork _unitOfWork = unitOfWork;
        private readonly UserManager<User> _userManager = userManager;
        private readonly IDistributedCache _cache = cache;
        private readonly ILogger<CreateProjectCommandHandler> _logger = logger;

        public async Task<Result<ProjectTileDto>> Handle(CreateProjectCommand request, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Handling Command");
            // Validate the request
            if (request is null || request.UserId == Guid.Empty || string.IsNullOrWhiteSpace(request.Title))
                return Result<ProjectTileDto>.Failure("Invalid request.");

            // Check if the user exists
            var user = await _userManager.FindByIdAsync(request.UserId.ToString());
            if (user is null)
                return Result<ProjectTileDto>.Failure("User not found.");

            // Validate and create value objects
            var projectTitleResult = Title.Create(request.Title);

            if (projectTitleResult.IsFailure)
                return Result<ProjectTileDto>.Failure(projectTitleResult.ErrorMessage ?? "Invalid project title.");


            var projectDescriptionResult = Description.Create(request.Description!);

            if (projectDescriptionResult.IsFailure)
                return Result<ProjectTileDto>.Failure(projectDescriptionResult.ErrorMessage ?? "Invalid project description.");

            _logger.LogInformation("Creating Project");

            // Create the project entity
            var projectResult = Project.Create(projectTitleResult.Value, projectDescriptionResult.Value, request.UserId);

            if (projectResult.IsFailure)
                return Result<ProjectTileDto>.Failure(projectResult.ErrorMessage ?? "Failed to create project.");

            var projectTileDto = new ProjectTileDto
            {
                Id = projectResult.Value.Id,
                Title = projectResult.Value.Title,
                Description = projectResult.Value.Description,
                TotalTodoItemCount = projectResult.Value.TodoItems.Count,
                CompleteTodoItemCount = projectResult.Value.TodoItems.Where(t => t.Status == Domain.Enums.Status.Complete).Count(),
                CreatedOn = projectResult.Value.CreatedOn,
            };

            // Save the project to the database
            try
            {
                _logger.LogInformation("Saving Project To Db");

                _unitOfWork.ProjectRepository.Add(projectResult.Value);
                await _unitOfWork.SaveChangesAsync(cancellationToken);

                _logger.LogInformation("Clearing User Redis Tiles Cache");
                string key = CacheKeys.ProjectTiles(user.Id);
                _cache.Remove(key); 

                return Result<ProjectTileDto>.Success(projectTileDto); 
            }

            catch (Exception ex)
            {
                _logger.LogError(ex, "Issue Creating Project");
                return Result<ProjectTileDto>.Failure("An error occurred while saving the project.");
            }
        }
    }
}
