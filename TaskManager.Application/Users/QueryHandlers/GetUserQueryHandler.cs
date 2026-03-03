using MediatR;
using Microsoft.AspNetCore.Identity;
using TaskManager.Application.Users.DTOs.Responses;
using TaskManager.Application.Users.Queries;
using TaskManager.Domain.Common;
using TaskManager.Domain.Entities;
using TaskManager.Domain.Interfaces;

namespace TaskManager.Application.Users.QueryHandlers
{
    public class GetUserQueryHandler(IUnitOfWork unitOfWork, UserManager<User> userManager) : IRequestHandler<GetUserQuery, Result<GetUserResponse>>
    {
        private readonly IUnitOfWork _unitOfWork = unitOfWork;
        private readonly UserManager<User> _userManager = userManager;
        public async Task<Result<GetUserResponse>> Handle(GetUserQuery request, CancellationToken cancellationToken)
        {
            //Validate Request
            if (request is null || string.IsNullOrWhiteSpace(request.Email))
                return Result<GetUserResponse>.Failure("Invalid Request");


            //Search For & Validate User
            var user = await _userManager.FindByEmailAsync(request.Email); 

            if (user is null || user.Id == Guid.Empty || string.IsNullOrWhiteSpace(user.Email) || string.IsNullOrWhiteSpace(user.FirstName) || string.IsNullOrWhiteSpace(user.LastName))
                return Result<GetUserResponse>.Failure("Cannot Find User");

            var foundUser = new GetUserResponse
            {
                //Id, FirstName, LastName, Email
                Id = user.Id,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Email = user.Email
            };

            return Result<GetUserResponse>.Success(foundUser); 

        }
    }
}
