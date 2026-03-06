using MediatR;
using TaskManager.Application.Common;
using TaskManager.Application.Interfaces;
using TaskManager.Application.Projects.DTOs;
using TaskManager.Domain.Common;

namespace TaskManager.Application.Projects.Commands
{
    public record UpdateProjectCommand(
       Guid UserId,
       Guid ProjectId,
       string? NewTitle,
       string? NewDescription
    ) : IRequest<Result<ProjectDetailsDto>>, ICacheInvalidator
    {
        public string[] Keys => [CacheKeys.ProjectTiles(UserId), CacheKeys.ProjectDetailedViews(UserId, ProjectId)];
    }
}
