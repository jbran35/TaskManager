using MediatR;
using System;
using System.Collections.Generic;
using System.Text;
using TaskManager.Application.Users.DTOs.Responses;

namespace TaskManager.Application.Users.Commands
{
    //UserName, Password, Email, FirstName, LastName

    public record RegisterUserCommand(
        string UserName, 
        string Password, 
        string Email, 
        string FirstName, 
        string LastName
        
        ) : IRequest<RegisterUserResponse>;
}
