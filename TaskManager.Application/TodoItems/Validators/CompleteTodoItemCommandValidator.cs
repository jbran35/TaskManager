using FluentValidation;
using TaskManager.Application.TodoItems.Commands;

namespace TaskManager.Application.TodoItems.Validators
{
    public class CompleteTodoItemCommandValidator : AbstractValidator<UpdateTodoItemStatusCommand>
    {
        public CompleteTodoItemCommandValidator()
        {

            RuleFor(x => x.UserId)
               .NotEmpty()
               .WithMessage("Your ID Is Required To Complete This Task");

            RuleFor(x => x.TodoItemId)
            .NotEmpty()
            .WithMessage("This Tasks's ID Is Required To Complete It");

        }
    }
}
