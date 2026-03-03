using FluentValidation;
using TaskManager.Application.TodoItems.Commands;

namespace TaskManager.Application.TodoItems.Validators
{
    public class AssignTodoItemCommandValidator : AbstractValidator<AssignTodoItemCommand>
    {
        public AssignTodoItemCommandValidator() 
        {
            RuleFor(x => x.UserId)
                .NotEmpty()
                .WithMessage("Your ID Is Required To Assign A Task To Another User");

            RuleFor(x => x.ProjectId)
                .NotEmpty()
                .WithMessage("This Project's ID Is Required To Assign A Task To Another User");

            RuleFor(x => x.TodoItemId)
              .NotEmpty()
              .WithMessage("This Tasks's ID Is Required To Assign It To Another User");

            RuleFor(x => x.AssigneeId)
            .NotEmpty()
            .WithMessage("Assignee's ID Is Required To Assign This Task To Them");
        }
    }
}
