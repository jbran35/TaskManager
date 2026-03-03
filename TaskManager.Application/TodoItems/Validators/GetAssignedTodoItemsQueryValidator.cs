using FluentValidation;
using TaskManager.Application.TodoItems.Queries;

namespace TaskManager.Application.TodoItems.Validators
{
    public class GetAssignedTodoItemsQueryValidator : AbstractValidator<GetAssignedTodoItemsQuery>
    {
        public GetAssignedTodoItemsQueryValidator()
        {
            RuleFor(x => x.UserId)
                .NotEmpty()
                .WithMessage("Your ID Is Required To Get Your Assigned Tasks");

        }
    }
}
