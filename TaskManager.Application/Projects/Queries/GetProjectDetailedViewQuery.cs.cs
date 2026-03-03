using MediatR;
using TaskManager.Application.Projects.DTOs;
using TaskManager.Domain.Common;

namespace TaskManager.Application.Projects.Queries
{
    public record GetProjectDetailedViewQuery(Guid UserId, Guid ProjectId) : IRequest<Result<ProjectDetailedViewDto>>;
}
