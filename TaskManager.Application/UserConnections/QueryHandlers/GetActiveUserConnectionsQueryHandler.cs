using MediatR;
using TaskManager.Application.UserConnections.DTOs;
using TaskManager.Application.UserConnections.Queries;
using TaskManager.Domain.Common;
using TaskManager.Domain.Interfaces;

namespace TaskManager.Application.UserConnections.QueryHandlers
{
    public class GetActiveUserConnectionsQueryHandler(IUnitOfWork unitOfWork) : IRequestHandler<GetActiveUserConnectionsQuery, Result<IEnumerable<UserConnectionDto>>>
    {
        private readonly IUnitOfWork _unitOfWork = unitOfWork;
        public async Task<Result<IEnumerable<UserConnectionDto>>> Handle(GetActiveUserConnectionsQuery request, CancellationToken cancellationToken)
        {
            //Validate Request
            if (request is null || request.UserId == Guid.Empty)
                return Result<IEnumerable<UserConnectionDto>>.Failure("Invalid Request");

            //Validate Connections
            var connections = await _unitOfWork.UserConnectionRepository.GetConnectionsByOwnerIdAsync(request.UserId, cancellationToken);

            if (connections is null)
                return Result<IEnumerable<UserConnectionDto>>.Failure("Issue Loading Assignees");

            //Map to DTOs

            var connectionDtos = connections.Select(static c => new UserConnectionDto
            (
                c.Id, 
                c.UserId,
                c.AssigneeId,
                c.Assignee?.FullName ?? string.Empty,
                c.Assignee?.Email ?? string.Empty
                )).ToList();

            return Result<IEnumerable<UserConnectionDto>>.Success(connectionDtos); 
        }
    }
}
