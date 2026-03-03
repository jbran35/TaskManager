using FluentValidation;
using TaskManager.Application.UserConnections.Commands;

namespace TaskManager.Application.UserConnections.Validators
{
    public class DeleteUserConnectionCommandValidator : AbstractValidator<DeleteUserConnectionCommand>
    {
        public DeleteUserConnectionCommandValidator() 
        {
            RuleFor(x => x.UserId)
                .NotEmpty()
                .WithMessage("Your ID Is Required To Remove This Assignee From Your Group");

            RuleFor(x => x.ConnectionId)
                .NotEmpty()
                .WithMessage("The Connection's ID Is Required To Remove This Assignee From Your Group");




        }
    }
}
