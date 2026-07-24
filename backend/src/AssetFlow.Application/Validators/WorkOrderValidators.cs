using AssetFlow.Application.Contracts.WorkOrders;
using FluentValidation;

namespace AssetFlow.Application.Validators;

public class CreateWorkOrderRequestValidator : AbstractValidator<CreateWorkOrderRequest>
{
    public CreateWorkOrderRequestValidator()
    {
        RuleFor(x => x.Title)
            .NotEmpty()
            .MaximumLength(200);

        RuleFor(x => x.Description)
            .MaximumLength(2000);

        RuleFor(x => x.Priority)
            .IsInEnum();

        RuleFor(x => x.AssetId)
            .NotEmpty().WithMessage("A valid asset id is required.");

        RuleFor(x => x.AssignedTechnicianId!.Value)
            .NotEmpty()
            .When(x => x.AssignedTechnicianId.HasValue);
    }
}

public class UpdateWorkOrderRequestValidator : AbstractValidator<UpdateWorkOrderRequest>
{
    public UpdateWorkOrderRequestValidator()
    {
        RuleFor(x => x.Title)
            .NotEmpty()
            .MaximumLength(200);

        RuleFor(x => x.Description)
            .MaximumLength(2000);

        RuleFor(x => x.Priority)
            .IsInEnum();

        RuleFor(x => x.Status)
            .IsInEnum();

        RuleFor(x => x.AssignedTechnicianId!.Value)
            .NotEmpty()
            .When(x => x.AssignedTechnicianId.HasValue);
    }
}
