using MediatR;
using TaskManager.Application.Projects.DTOs.Responses;
using TaskManager.Domain.Common;

namespace TaskManager.Application.Projects.Commands
{

    public record DeleteProjectCommand(
    Guid UserId,
    Guid ProjectId
) : IRequest<Result<DeleteProjectResponse>>;
}
