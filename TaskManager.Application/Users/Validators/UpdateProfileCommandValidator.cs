using FluentValidation;
using TaskManager.Application.Users.Commands;

namespace TaskManager.Application.Users.Validators
{
    public class UpdateProfileCommandValidator : AbstractValidator<UpdateProfileCommand>
    {
        public UpdateProfileCommandValidator() 
        {

            RuleFor(x => x.NewEmail)
                .NotNull()
                .NotEmpty()
                .EmailAddress()
                .When(x => !string.IsNullOrWhiteSpace(x.NewEmail))
                .WithMessage("Email Address Must Be In Email Format");


            RuleFor(x => x.NewUserName)
                .NotNull()
                .NotEmpty()
                .Matches("^\\S*")
                .When(x => !string.IsNullOrWhiteSpace(x.NewEmail))
                .WithMessage("Your New UserName Cannot Contain Any Spaces");

        }
    }
}
