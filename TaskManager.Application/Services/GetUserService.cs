using Microsoft.AspNetCore.Identity;
using TaskManager.Application.DTOs;
using TaskManager.Application.Interfaces;
using TaskManager.Domain.Entities;

namespace TaskManager.Application.Services
{
    public class GetUserService : IGetUserService
    {
        private readonly UserManager<User> _userManager;

        public GetUserService(UserManager<User> userManager)
        {
            _userManager = userManager;
        }

        //Gets User by their Id
        public async Task<GetUserResponse> GetUserByIdAsync(Guid userId)
        {
            //Verify input 
            if (userId == Guid.Empty)
            {
                return new GetUserResponse
                {
                    Success = false,
                    Message = "Invalid user ID"
                };
            }

            //Find user by ID, ensure they exist
            var user = await _userManager.FindByIdAsync(userId.ToString());
            
            if (user is null || user is null)
            {
                return new GetUserResponse
                {
                    Success = false,
                    Message = "User not found"
                };
            }

            return new GetUserResponse
            {
                UserId = user.Id,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Email = user.Email ?? string.Empty,
                Success = true,
                Message = "User found"
            };
        }

        //Gets User by their email
        public async Task<GetUserResponse> GetUserByEmailAsync(string userEmail)
        {
            //Verify input
            if (string.IsNullOrWhiteSpace(userEmail))
            {
                return new GetUserResponse
                {
                    Success = false,
                    Message = "Invalid email address"
                };
            }

            //Find user by email, ensure they exist
            var user = await _userManager.FindByEmailAsync(userEmail);

            if (user is null || user is null) {
                
                return new GetUserResponse
                {
                    Success = false,
                    Message = "User not found"
                };
            }

            //Return user details

            return new GetUserResponse
            {
                UserId = user.Id,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Email = userEmail,
                Success = true,
                Message = "User found"
            };
        }
    }
}
