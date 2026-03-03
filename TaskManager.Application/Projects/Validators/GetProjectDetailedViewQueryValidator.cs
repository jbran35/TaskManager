using FluentValidation;
using TaskManager.Application.Projects.Queries;

namespace TaskManager.Application.Projects.Validators
{
    public class GetProjectDetailedViewQueryValidator : AbstractValidator<GetProjectDetailedViewQuery>
    {
        public GetProjectDetailedViewQueryValidator()
        {
            RuleFor(x => x.UserId)
                .NotEmpty()
                .WithMessage("Cannot Retreive A Project's Details Without Your ID");

            RuleFor(x => x.ProjectId)
                .NotEmpty()
                .WithMessage("Cannot Retrieve A Project's Details Without Its ID");
        }
    }
}
