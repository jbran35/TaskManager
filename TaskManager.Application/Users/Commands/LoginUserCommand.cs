using MediatR;
using TaskManager.Application.Users.DTOs.Responses;

namespace TaskManager.Application.Users.Commands
{
    public record LoginUserCommand(
        string UserName, 
        string Password) : IRequest<LoginUserResponse>;
}
