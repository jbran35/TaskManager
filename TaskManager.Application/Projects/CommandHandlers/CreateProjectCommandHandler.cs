using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using TaskManager.Application.Projects.Commands;
using TaskManager.Application.Projects.DTOs;
using TaskManager.Domain.Common;
using TaskManager.Domain.Entities;
using TaskManager.Domain.Interfaces;
using TaskManager.Domain.ValueObjects;

namespace TaskManager.Application.Projects.CommandHandlers
{
    public class CreateProjectCommandHandler(
        IUnitOfWork unitOfWork, UserManager<User> userManager, 
        ILogger<CreateProjectCommandHandler> logger) : IRequestHandler<CreateProjectCommand, Result<ProjectDetailsDto>>
    {
        private readonly IUnitOfWork _unitOfWork = unitOfWork;
        private readonly UserManager<User> _userManager = userManager;
        private readonly ILogger<CreateProjectCommandHandler> _logger = logger;
        public async Task<Result<ProjectDetailsDto>> Handle(CreateProjectCommand request, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Handling Command");
            if (request is null || request.UserId == Guid.Empty || string.IsNullOrWhiteSpace(request.Title))
                return Result<ProjectDetailsDto>.Failure("Invalid request.");

            var user = await _userManager.FindByIdAsync(request.UserId.ToString());
            if (user is null)
                return Result<ProjectDetailsDto>.Failure("User not found.");

            var projectTitleResult = Title.Create(request.Title);

            if (projectTitleResult.IsFailure)
                return Result<ProjectDetailsDto>.Failure(projectTitleResult.ErrorMessage ?? "Invalid project title.");

            var projectDescriptionResult = Description.Create(request.Description!);

            if (projectDescriptionResult.IsFailure)
                return Result<ProjectDetailsDto>.Failure(projectDescriptionResult.ErrorMessage ?? "Invalid project description.");

            _logger.LogInformation("Creating Project");

            var projectResult = Project.Create(projectTitleResult.Value, projectDescriptionResult.Value, request.UserId);

            if (projectResult.IsFailure)
                return Result<ProjectDetailsDto>.Failure(projectResult.ErrorMessage ?? "Failed to create project.");

            var projectDetails = new ProjectDetailsDto
            {
                Id = projectResult.Value.Id,
                Title = projectResult.Value.Title,
                Description = projectResult.Value.Description,
                CreatedOn = projectResult.Value.CreatedOn,
            };

            // Save the project to the database
            try
            {
                _logger.LogInformation("Saving Project To Db");
                _unitOfWork.ProjectRepository.Add(projectResult.Value);
                await _unitOfWork.SaveChangesAsync(cancellationToken);
                return Result<ProjectDetailsDto>.Success(projectDetails); 
            }

            catch (Exception ex)
            {
                _logger.LogError(ex, "Issue Creating Project");
                return Result<ProjectDetailsDto>.Failure("An error occurred while saving the project.");
            }
        }
    }
}
