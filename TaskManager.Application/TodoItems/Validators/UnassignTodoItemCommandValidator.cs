using FluentValidation;
using TaskManager.Application.TodoItems.Commands;

namespace TaskManager.Application.TodoItems.Validators
{
    public class UnassignTodoItemCommandValidator : AbstractValidator<UnassignTodoItemCommand>
    {
        public UnassignTodoItemCommandValidator()
        {
            RuleFor(x => x.UserId)
                .NotNull()
                .NotEmpty()
                .WithMessage("Your ID Is Required To Unassign This Task");

            RuleFor(x => x.ProjectId)
              .NotNull()
              .NotEmpty()
              .WithMessage("This Project's ID Is Required To Unassign This Task");

            RuleFor(x => x.TodoItemId)
              .NotNull()
              .NotEmpty()
              .WithMessage("This Task's ID Is Required To Unassign It");



        }
    }
}
