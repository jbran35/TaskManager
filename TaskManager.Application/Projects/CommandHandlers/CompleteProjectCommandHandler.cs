using MediatR;
using Microsoft.AspNetCore.Identity;
using TaskManager.Application.Projects.Commands;
using TaskManager.Application.Projects.DTOs;
using TaskManager.Application.Projects.DTOs.Responses;
using TaskManager.Domain.Common;
using TaskManager.Domain.Entities;
using TaskManager.Domain.Interfaces;

namespace TaskManager.Application.Projects.CommandHandlers
{
    public class CompleteProjectCommandHandler(IUnitOfWork unitOfWork, UserManager<User> userManager) : IRequestHandler<CompleteProjectCommand, Result<CompleteProjectResponse>>
    {
        private readonly IUnitOfWork _unitOfWork = unitOfWork;
        private readonly UserManager<User> _userManager = userManager;
        public async Task<Result<CompleteProjectResponse>> Handle(CompleteProjectCommand request, CancellationToken cancellationToken)
        {
            // Validate the request
            if (request is null || request.UserId == Guid.Empty || request.ProjectId == Guid.Empty)
                return Result<CompleteProjectResponse>.Failure("Invalid request.");

            // Validate the user
            var user = await _userManager.FindByIdAsync(request.UserId.ToString());
            if (user is null)
                return Result<CompleteProjectResponse>.Failure("User not found.");


            // Retrieve the project and ensure it exists and belongs to the user
            var project = await _unitOfWork.ProjectRepository.GetProjectWithTasksAsync(request.ProjectId, cancellationToken);

            if(project is null)
                return Result<CompleteProjectResponse>.Failure("Project not found.");

            if(request.UserId != project.OwnerId)
                return Result<CompleteProjectResponse>.Failure("Unauthorized.");


            // Mark the project as complete
            var result = project.MarkAsComplete();

             if(result.IsFailure)
                return Result<CompleteProjectResponse>.Failure(result.ErrorMessage!);

            // Update the project in the repository
            try
            {
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

                return Result<CompleteProjectResponse>.Success(response);

            }
            catch (Exception)
            {
                return Result<CompleteProjectResponse>.Failure("An error occurred while completing the project.");
            }
        }
    }
}
