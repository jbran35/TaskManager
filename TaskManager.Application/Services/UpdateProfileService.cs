using Microsoft.AspNetCore.Identity;
using TaskManager.Application.DTOs;
using TaskManager.Application.Interfaces;
using TaskManager.Domain.Entities;
using TaskManager.Domain.Interfaces;

namespace TaskManager.Application.Services
{
    public class UpdateProfileService(IUnitOfWork unitOfWork, UserManager<User> userManager) : IUpdateProfileService
    {
        private readonly IUnitOfWork _unitOfWork = unitOfWork;
        private readonly UserManager<User> _userManager = userManager;
       
        public async Task<UpdateProfileResponse> UpdateProfileAsync(UpdateProfileRequest request)
        {
            if(request is null || request.Id == Guid.Empty)
            {
                return new UpdateProfileResponse
                {
                    Message = "Bad Request"
                }; 
            }

            var user = await _userManager.FindByIdAsync(request.Id.ToString());
            
            if (user is null)
            {
                return new UpdateProfileResponse
                {
                    Message = "User Not Found"
                }; 
            }

            //Verify that the new email and username aren't already used


            if (!string.IsNullOrWhiteSpace(request.FirstName))
            {
                user.FirstName = request.FirstName;
            }
            

            if (!string.IsNullOrWhiteSpace(request.LastName))
            {
                user.LastName = request.LastName;

            }

            if (!string.IsNullOrWhiteSpace(request.Email))
            {
                if(user.Email != request.Email)

                {
                    Console.WriteLine("Different email detected");

                    var emailUser = _userManager.FindByEmailAsync(request.Email);
                    Console.WriteLine("Pulled user with new email");

                    if(emailUser is null)
                    {
                        Console.WriteLine("Pulled user is null");
                        user.Email = request.Email;
                    }
                    else
                    {
                        return new UpdateProfileResponse{ Message = "Email already taken by another user" };
                    }
                }
            }

            if (!string.IsNullOrWhiteSpace(request.UserName))
            {
                if (user.UserName != request.UserName)
                {
                    var userNameUser = _userManager.FindByNameAsync(request.UserName);

                    if (userNameUser is null)
                    {
                        user.UserName = request.UserName;
                    }
                    else
                    {
                        return new UpdateProfileResponse { Message = "Username already taken by another user" };

                    }
                }
            }

            try
            {
                var response = await _userManager.UpdateAsync(user);
            }

            catch (Exception ex)
            {
                Console.WriteLine("Error Updating User: \n" + ex.Message);
            }

            return new UpdateProfileResponse { 
                NewFirstName = user.FirstName, 
                NewLastName = user.LastName, 
                NewEmail = user.Email ?? string.Empty, 
                NewUserName = user.UserName ?? string.Empty,
                Success = true, Message= "Profile Updated Successfully!" };  

            }
        }
    }
