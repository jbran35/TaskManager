using MediatR;
using Microsoft.AspNetCore.Identity;
using TaskManager.Application.Users.Commands;
using TaskManager.Application.Users.DTOs.Responses;
using TaskManager.Domain.Entities;

namespace TaskManager.Application.Users.CommandHandlers
{
    public class RegisterUserHandler(UserManager<User> userManager) : IRequestHandler<RegisterUserCommand, RegisterUserResponse>
    {
        private readonly UserManager<User> _userManager = userManager;

        public async Task<RegisterUserResponse> Handle(RegisterUserCommand request, CancellationToken cancellationToken)
        {
            // Validate the request
            if (request is null)
            {
                return new RegisterUserResponse

                {
                    Success = false,
                    Message = "Invalid registration request"
                };
            }

            // Check if the username or email already exists
            var userByUsername = await _userManager.FindByNameAsync(request.UserName);

            if (userByUsername != null)
            {
                return new RegisterUserResponse
                {
                    Success = false,
                    Message = "User already exists"
                };
            }

            // Check if the email is already in use
            var userByEmail = await _userManager.FindByEmailAsync(request.Email);
            if (userByEmail != null)
            {
                return new RegisterUserResponse
                {
                    Success = false,
                    Message = "Email already in use"
                };
            }

            // Create a new user
            var user = new User
            {
                UserName = request.UserName,
                Email = request.Email,
                FirstName = request.FirstName,
                LastName = request.LastName
            };

            var result = await _userManager.CreateAsync(user, request.Password);

            if (!result.Succeeded)
            {
                var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                return new RegisterUserResponse
                {
                    Success = false,
                    Message = $"User creation failed: {errors}"
                };
            }

            return new RegisterUserResponse
            {
                Success = true,
                Message = "User created successfully"
            };

        }

    }
}
