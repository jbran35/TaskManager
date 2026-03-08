using FluentValidation;
using TaskManager.Application.TodoItems.Queries;

namespace TaskManager.Application.TodoItems.Validators
{
    public class GetTodoItemDetailedViewQueryValidator : AbstractValidator<GetTodoItemDetailedViewQuery>
    {
        public GetTodoItemDetailedViewQueryValidator()
        {
            RuleFor(x => x.UserId)
                .NotNull()
                .NotEmpty()
                .WithMessage("Your ID Is Required To Get This Task's Details");

            RuleFor(x => x.TodoItemId)
                .NotNull()
                .NotEmpty()
                .WithMessage("This Project's ID Is Required To Retrieve This Task's Details");

            RuleFor(x => x.ProjectId)
                .NotNull()
                .NotEmpty()
                .WithMessage("ProjectID Required");
        }
    }
}
