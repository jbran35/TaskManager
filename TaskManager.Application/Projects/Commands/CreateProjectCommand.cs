using MediatR;
using TaskManager.Application.Common;
using TaskManager.Application.Interfaces;
using TaskManager.Application.Projects.DTOs;
using TaskManager.Domain.Common;

namespace TaskManager.Application.Projects.Commands
{
    public record CreateProjectCommand(
        Guid UserId,
        string? Title,
        string? Description) : IRequest<Result<ProjectTileDto>>, ICacheInvalidator
    {
        public string[] Keys => [CacheKeys.ProjectTiles(UserId)];
    }
}
