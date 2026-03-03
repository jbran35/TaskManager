using MediatR;
using Microsoft.AspNetCore.Identity;
using TaskManager.Application.DTOs;
using TaskManager.Application.Interfaces;
using TaskManager.Application.Users.Commands;
using TaskManager.Application.Users.DTOs.Responses;
using TaskManager.Domain.Entities;

namespace TaskManager.Application.Users.CommandHandlers
{
    public class LoginUserHandler
        (UserManager<User> userManager, SignInManager<User> signInManager, ITokenService tokenService) : IRequestHandler<LoginUserCommand, LoginUserResponse>

    {
            private readonly UserManager<User> _userManager = userManager;
            private readonly SignInManager<User> _signInManager = signInManager;
            private readonly ITokenService _tokenService = tokenService;

        //public async Task<LoginUserResponse> AttemptLogin(LoginUserRequest request)
        //{
        //    //Verify input
        //    if(request is null)
        //    {
        //        return new LoginUserResponse
        //        {
        //            Token = null,
        //            Success = false,
        //            Message = "Invalid login request"
        //        };
        //    }

        //    //Find user by username, ensure they exist
        //    var user = await _userManager.FindByNameAsync(request.UserName);

        //    if (user is null)
        //    {
        //        return new LoginUserResponse
        //        {
        //            Token = null,
        //            Success = false,
        //            Message = "User not found"
        //        };
        //    }

        //    var result = await _signInManager.CheckPasswordSignInAsync(user, request.Password, false);

        //    if (!result.Succeeded)
        //    {
        //        return new LoginUserResponse
        //        {
        //            Token = null,
        //            Success = false,
        //            Message = "Invalid Username or Password"
        //        };
        //    }

        //    //Generate & return JWT token
        //    var token = _tokenService.CreateToken(user);

        //    Console.WriteLine("In LoginHandler: " + user.Email);

        //    Console.WriteLine("user.Email: " + user.Email);
        //    return new LoginUserResponse
        //    {
        //        Id = user.Id,
        //        UserName = user.UserName ?? string.Empty,
        //        FirstName = user.FirstName, 
        //        LastName = user.LastName,
        //        Email = user.Email ?? string.Empty,
        //        Token = token,
        //        Success = true,
        //        Message = "User logged in successfully"
        //    };
        //}

        public async Task<LoginUserResponse> Handle(LoginUserCommand request, CancellationToken cancellationToken)
        {
            //Verify input
            if (request is null)
            {
                return new LoginUserResponse
                {
                    Token = null,
                    Success = false,
                    Message = "Invalid login request"
                };
            }

            //Find user by username, ensure they exist
            var user = await _userManager.FindByNameAsync(request.UserName);

            if (user is null)
            {
                return new LoginUserResponse
                {
                    Token = null,
                    Success = false,
                    Message = "User not found"
                };
            }

            var result = await _signInManager.CheckPasswordSignInAsync(user, request.Password, false);

            if (!result.Succeeded)
            {
                return new LoginUserResponse
                {
                    Token = null,
                    Success = false,
                    Message = "Invalid Username or Password"
                };
            }

            //Generate & return JWT token
            var token = _tokenService.CreateToken(user);

            Console.WriteLine("In LoginHandler: " + user.Email);

            Console.WriteLine("user.Email: " + user.Email);
            return new LoginUserResponse
            {
                Id = user.Id,
                UserName = user.UserName ?? string.Empty,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Email = user.Email ?? string.Empty,
                Token = token,
                Success = true,
                Message = "User logged in successfully"
            };
        }
    }
}
