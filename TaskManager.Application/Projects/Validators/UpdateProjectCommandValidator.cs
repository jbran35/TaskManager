using FluentValidation;
using TaskManager.Application.Projects.Commands;

namespace TaskManager.Application.Projects.Validators
{
    public class UpdateProjectCommandValidator : AbstractValidator<UpdateProjectCommand>
    {
        public UpdateProjectCommandValidator()
        {
            RuleFor(x => x.UserId)
                .NotEmpty()
                .WithMessage("Your ID Is Required To Update It");

            RuleFor(x => x.ProjectId)
                .NotEmpty()
                .WithMessage("This Project's ID Is Required To Update It");

            RuleFor(x => x.NewTitle)
                .NotNull()
                .NotEmpty()
                .WithMessage("Cannot Save A Project Without A Title");
        }
    }
}
