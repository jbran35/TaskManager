using MediatR;
using TaskManager.Application.Projects.DTOs;
using TaskManager.Application.Projects.DTOs.Responses;
using TaskManager.Domain.Common;

namespace TaskManager.Application.Projects.Queries
{
    public record GetUserProjectsQuery(Guid UserId) : IRequest<Result<List<ProjectTileDto>>>;
}