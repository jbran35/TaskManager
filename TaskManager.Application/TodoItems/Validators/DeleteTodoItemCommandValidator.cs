using FluentValidation;
using TaskManager.Application.TodoItems.Commands;

namespace TaskManager.Application.TodoItems.Validators
{
    public class DeleteTodoItemCommandValidator : AbstractValidator<DeleteTodoItemCommand>
    {
        public DeleteTodoItemCommandValidator() 
        {
            RuleFor(x => x.UserId)
                .NotEmpty()
                .WithMessage("Your ID Is Required To Delete This Task");

            RuleFor(x => x.TodoItemId)
                .NotEmpty()
                .WithMessage("This Task's ID Is Required To Delete It");
        }
    }
}
