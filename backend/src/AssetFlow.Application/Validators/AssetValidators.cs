using AssetFlow.Application.Contracts.Assets;
using FluentValidation;

namespace AssetFlow.Application.Validators;

public class CreateAssetRequestValidator : AbstractValidator<CreateAssetRequest>
{
    public CreateAssetRequestValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty()
            .MaximumLength(200);

        RuleFor(x => x.Tag)
            .NotEmpty()
            .MaximumLength(100);

        RuleFor(x => x.Status)
            .IsInEnum();

        RuleFor(x => x.SiteId)
            .NotEmpty().WithMessage("A valid site id is required.");
    }
}

public class UpdateAssetRequestValidator : AbstractValidator<UpdateAssetRequest>
{
    public UpdateAssetRequestValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty()
            .MaximumLength(200);

        RuleFor(x => x.Tag)
            .NotEmpty()
            .MaximumLength(100);

        RuleFor(x => x.Status)
            .IsInEnum();

        RuleFor(x => x.SiteId)
            .NotEmpty().WithMessage("A valid site id is required.");
    }
}
