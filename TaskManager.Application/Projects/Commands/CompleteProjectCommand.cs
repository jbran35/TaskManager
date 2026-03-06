using MediatR;
using TaskManager.Application.Common;
using TaskManager.Application.Interfaces;
using TaskManager.Application.Projects.DTOs.Responses;
using TaskManager.Domain.Common;

namespace TaskManager.Application.Projects.Commands
{
    public record CompleteProjectCommand(
        Guid UserId,
        Guid ProjectId
    ) : IRequest<Result<CompleteProjectResponse>>, ICacheInvalidator
    {
        public string[] Keys => [CacheKeys.ProjectDetailedViews(UserId, ProjectId), CacheKeys.ProjectTiles(UserId)];
    }
}
