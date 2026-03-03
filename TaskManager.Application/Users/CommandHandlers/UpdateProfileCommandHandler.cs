using MediatR;
using Microsoft.AspNetCore.Identity;
using TaskManager.Application.Users.Commands;
using TaskManager.Application.Users.DTOs;
using TaskManager.Domain.Common;
using TaskManager.Domain.Entities;
using TaskManager.Domain.Interfaces;

namespace TaskManager.Application.Users.CommandHandlers
{
    public class UpdateProfileCommandHandler(IUnitOfWork unitOfWork, UserManager<User> userManager) : IRequestHandler<UpdateProfileCommand, Result<UserProfileDto>>
    {
        private readonly IUnitOfWork _unitOfWork = unitOfWork;
        private readonly UserManager<User> _userManager = userManager;
        public async Task<Result<UserProfileDto>> Handle(UpdateProfileCommand request, CancellationToken cancellationToken)
        {
            //Validate Request
            if (request is null || request.Id == Guid.Empty)
                return Result<UserProfileDto>.Failure("Inavlid Request");

            var user = await _userManager.FindByIdAsync(request.Id.ToString());

            if (user is null)
                return Result<UserProfileDto>.Failure("Account Not Found"); 

            //Change properties that are not null or empty
            if(request.NewFirstName is not null && request.NewFirstName != string.Empty && request.NewFirstName != user.FirstName)
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


            //Map new profile to DTO and return
            var newProfile = new UserProfileDto(
                user.Id,
                user.FirstName, 
                user.LastName,
                user.Email ?? string.Empty,
                user.UserName ?? string.Empty); 

            //Save FirstName/LastName changes to DB
            try
            {
                var updateResult = await _userManager.UpdateAsync(user); 
                if (!updateResult.Succeeded)
                    return Result<UserProfileDto>.Failure("Unexpected Error Updating Your Profile");
                await _unitOfWork.SaveChangesAsync(cancellationToken); 
            }

            catch (Exception)
            {
                return Result<UserProfileDto>.Failure("Unexpected Error Updating Your Profile");
            }

            return Result<UserProfileDto>.Success(newProfile); 
        }
    }
}
