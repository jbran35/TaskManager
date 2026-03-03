using FluentValidation;
using TaskManager.Application.Projects.Commands;

namespace TaskManager.Application.Projects.Validators
{
    public class UpdateProjectDescriptionCommandValidator : AbstractValidator<UpdateProjectCommand>
    {
        public UpdateProjectDescriptionCommandValidator()
        {
            RuleFor(x => x.UserId)
                .NotEmpty()
                .WithMessage("Your ID Is Required To Update It");

            RuleFor(x => x.ProjectId)
                .NotEmpty()
                .WithMessage("This Project's ID Is Required To Update It");
        }
    }
}
