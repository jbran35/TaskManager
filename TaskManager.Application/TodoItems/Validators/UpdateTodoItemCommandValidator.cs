using FluentValidation;
using TaskManager.Application.TodoItems.Commands;

namespace TaskManager.Application.TodoItems.Validators
{
    public class UpdateTodoItemCommandValidator : AbstractValidator<UpdateTodoItemCommand>
    {
        public UpdateTodoItemCommandValidator() 
        {
            RuleFor(x => x.UserId)
                .NotEmpty()
                .WithMessage("Your ID Is Required To Update This Task");

            RuleFor(x => x.ProjectId)
                .NotEmpty()
                .WithMessage("This Project's ID Is Required To Update This Task");

            RuleFor(x => x.TodoItemId)
              .NotEmpty()
              .WithMessage("This Task's ID Is Required To Update It");


            RuleFor(x => x.NewTitle)
              .NotEmpty()
              .When(x => !string.IsNullOrWhiteSpace(x.NewTitle))
              .WithMessage("This Task's ID Is Required To Update It");

        }
    }
}
