using FluentValidation;
using TaskManager.Application.Projects.Commands;

namespace TaskManager.Application.Projects.Validators
{
    public class DeleteProjectCommandValidator : AbstractValidator<DeleteProjectCommand>
    {
        public DeleteProjectCommandValidator()
        {
            RuleFor(x => x.UserId)
                .NotEmpty()
                .WithMessage("Your ID Is Required To Delete This Project");

            RuleFor(x => x.ProjectId)
               .NotEmpty()
               .WithMessage("This Project's ID Is Required To Delete It");
        }
    }
}
