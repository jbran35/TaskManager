using FluentValidation;
using TaskManager.Application.Projects.Commands;

namespace TaskManager.Application.Projects.Validators
{
    public class AddTodoItemCommandValidator : AbstractValidator<AddTodoItemCommand>
    {
        public AddTodoItemCommandValidator()
        {
            RuleFor(x => x.UserId)
                .NotEmpty()
                .WithMessage("Your ID Is Required To Create A Task");

            RuleFor(x => x.ProjectId)
                .NotEmpty()
                .WithMessage("This Project's ID Is Required To Create A Task");

            RuleFor(x => x.Title)
                .NotNull()
                .NotEmpty()
                .WithMessage("A Title Is Required To Create A Task");

            RuleFor(x => x.Priority)
                .IsInEnum()
                .WithMessage("The Selected Priority Is Not A Valid Option");
        }
    }
}
