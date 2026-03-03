using FluentValidation;
using TaskManager.Application.UserConnections.Queries;

namespace TaskManager.Application.UserConnections.Validators
{
    public class GetActiveUserConnectionsQueryValidator : AbstractValidator<GetActiveUserConnectionsQuery>
    {
        public GetActiveUserConnectionsQueryValidator()
        {
            RuleFor(x => x.UserId)
                .NotEmpty()
                .WithMessage("Your ID Is Required To Retrieve Your Group"); 
        }
    }
}
