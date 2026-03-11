using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using TaskManager.Application.Users.Commands;
using TaskManager.Application.Users.DTOs;
using TaskManager.Domain.Common;
using TaskManager.Domain.Entities;
using TaskManager.Domain.Interfaces;

namespace TaskManager.Application.Users.CommandHandlers
{
    public class UpdateProfileCommandHandler(IUnitOfWork unitOfWork, UserManager<User> userManager, ILogger<UpdateProfileCommandHandler> logger) 
        : IRequestHandler<UpdateProfileCommand, Result<UserProfileDto>>
    {
        private readonly IUnitOfWork _unitOfWork = unitOfWork;
        private readonly UserManager<User> _userManager = userManager;
        private readonly ILogger<UpdateProfileCommandHandler> _logger = logger;
        public async Task<Result<UserProfileDto>> Handle(UpdateProfileCommand request, CancellationToken cancellationToken)
        {
            _logger.LogInformation("In Handler");
            if (request is null || request.Id == Guid.Empty)
                return Result<UserProfileDto>.Failure("Inavlid Request");

            _logger.LogInformation("Request Validated");

            var user = await _userManager.FindByIdAsync(request.Id.ToString());
            if (user is null)
                return Result<UserProfileDto>.Failure("Account Not Found");

            _logger.LogInformation("User Validated");

            //Change properties that are not null or empty
            if (request.NewFirstName is not null && request.NewFirstName != string.Empty && request.NewFirstName != user.FirstName)
                user.FirstName = request.NewFirstName;

            if(request.NewLastName is not null && request.NewLastName != string.Empty && request.NewLastName != user.LastName)
                user.LastName = request.NewLastName;
            
            if (request.NewEmail is not null && request.NewEmail != string.Empty && request.NewEmail != user.Email)
            {
                var emailResult = await _userManager.SetEmailAsync(user, request.NewEmail);

                if (emailResult is null || !emailResult.Succeeded)
                    return Result<UserProfileDto>.Failure("Unexpected Error Updating Email"); 
            }

            if (request.NewUserName is not null && request.NewUserName != string.Empty && request.NewUserName != user.UserName)
            {
                var userNameResult = await _userManager.SetUserNameAsync(user, request.NewUserName);

                if(userNameResult is null || !userNameResult.Succeeded)
                    return Result<UserProfileDto>.Failure("Unexpected Error Updating UserName");
            }

            _logger.LogInformation("Done Changing Properties");

            //Map new profile to DTO and return
            var newProfile = new UserProfileDto(
                user.Id,
                user.FirstName, 
                user.LastName,
                user.Email ?? string.Empty,
                user.UserName ?? string.Empty);

            _logger.LogInformation("New Profile Details: ");
            _logger.LogInformation(newProfile.Id.ToString());
            _logger.LogInformation(newProfile.FirstName);
            _logger.LogInformation(newProfile.LastName);
            _logger.LogInformation(newProfile.Email);
            _logger.LogInformation(newProfile.UserName);

            try
            {
                _logger.LogInformation("Updating & Saving");

                var updateResult = await _userManager.UpdateAsync(user); 

                if (!updateResult.Succeeded)
                    return Result<UserProfileDto>.Failure("Unexpected Error Updating Your Profile");

                await _unitOfWork.SaveChangesAsync(cancellationToken); 
            }

            catch (Exception ex)
            {
                _logger.LogInformation(ex, "Error updating profile:");

                return Result<UserProfileDto>.Failure("Unexpected Error Updating Your Profile");
            }
            Console.WriteLine("About to return");

            return Result<UserProfileDto>.Success(newProfile); 
        }
    }
}
