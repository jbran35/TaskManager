using MediatR;
using TaskManager.Application.Users.DTOs;
using TaskManager.Domain.Common;

namespace TaskManager.Application.Users.Commands
{
    public record UpdateProfileCommand(
        Guid Id,
        string? NewFirstName,
        string? NewLastName,
        string? NewEmail,
        string? NewUserName
        ) : IRequest<Result<UserProfileDto>>;
  
}
