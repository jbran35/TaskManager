using MediatR;
using TaskManager.Application.Users.DTOs.Responses;
using TaskManager.Domain.Common;

namespace TaskManager.Application.Users.Queries
{
    public record GetUserQuery(string Email) : IRequest<Result<GetUserResponse>>;

}
