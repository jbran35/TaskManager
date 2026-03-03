using MediatR;
using TaskManager.Application.Projects.DTOs;
using TaskManager.Domain.Common;

namespace TaskManager.Application.Projects.Commands
{
    public record CreateProjectCommand(
        Guid UserId,
        string? Title,
        string? Description) : IRequest<Result<ProjectTileDto>>;
    
}
