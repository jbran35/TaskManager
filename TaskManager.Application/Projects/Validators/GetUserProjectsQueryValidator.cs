using FluentValidation;
using TaskManager.Application.Projects.Queries;

namespace TaskManager.Application.Projects.Validators
{
    public class GetUserProjectsQueryValidator : AbstractValidator<GetUserProjectsQuery>
    {
        public GetUserProjectsQueryValidator()
        {
            RuleFor(x => x.UserId)
                .NotEmpty()
                .WithMessage("Your ID Is Required To Retrieve Your Projects");
        }
    }
}
