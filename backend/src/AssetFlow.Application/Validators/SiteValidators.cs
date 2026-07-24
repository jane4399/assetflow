using AssetFlow.Application.Contracts.Sites;
using FluentValidation;

namespace AssetFlow.Application.Validators;

public class CreateSiteRequestValidator : AbstractValidator<CreateSiteRequest>
{
    public CreateSiteRequestValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty()
            .MaximumLength(200);

        RuleFor(x => x.Code)
            .NotEmpty()
            .MaximumLength(50)
            .Matches("^[A-Za-z0-9-]+$")
            .WithMessage("Code may contain only letters, digits and hyphens.");

        RuleFor(x => x.Location)
            .MaximumLength(300);
    }
}

public class UpdateSiteRequestValidator : AbstractValidator<UpdateSiteRequest>
{
    public UpdateSiteRequestValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty()
            .MaximumLength(200);

        RuleFor(x => x.Code)
            .NotEmpty()
            .MaximumLength(50)
            .Matches("^[A-Za-z0-9-]+$")
            .WithMessage("Code may contain only letters, digits and hyphens.");

        RuleFor(x => x.Location)
            .MaximumLength(300);
    }
}
