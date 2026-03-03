using FluentValidation;
using TaskManager.Application.Projects.Commands;

namespace TaskManager.Application.Projects.Validators
{
    public class CreateProjectCommandValidator : AbstractValidator<CreateProjectCommand>
    {
        public CreateProjectCommandValidator()
        {
            RuleFor(x => x.UserId)
                .NotEmpty()
                .WithMessage("Your ID Is Required To Create A Project");

            RuleFor(x => x.Title)
              .NotNull()
              .NotEmpty()
              .WithMessage("A Title Is Required To Create A Project");
        }
    }
}
