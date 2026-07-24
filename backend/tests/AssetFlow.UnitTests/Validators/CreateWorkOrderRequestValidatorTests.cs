using AssetFlow.Application.Contracts.WorkOrders;
using AssetFlow.Application.Validators;
using AssetFlow.Domain.Entities;
using FluentValidation.TestHelper;
using Xunit;

namespace AssetFlow.UnitTests.Validators;

public class CreateWorkOrderRequestValidatorTests
{
    private readonly CreateWorkOrderRequestValidator _validator = new();

    private static CreateWorkOrderRequest Valid() => new(
        Title: "Inspect valve",
        Description: "Routine inspection",
        Priority: WorkOrderPriority.Medium,
        AssetId: Guid.NewGuid(),
        AssignedTechnicianId: null,
        DueDate: DateTime.UtcNow.AddDays(1));

    [Fact]
    public void ValidRequest_PassesValidation()
    {
        var result = _validator.TestValidate(Valid());
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void MissingTitle_FailsValidation(string title)
    {
        var result = _validator.TestValidate(Valid() with { Title = title });
        result.ShouldHaveValidationErrorFor(x => x.Title);
    }

    [Fact]
    public void EmptyAssetId_FailsValidation()
    {
        var result = _validator.TestValidate(Valid() with { AssetId = Guid.Empty });
        result.ShouldHaveValidationErrorFor(x => x.AssetId);
    }

    [Fact]
    public void UndefinedPriority_FailsValidation()
    {
        var result = _validator.TestValidate(Valid() with { Priority = (WorkOrderPriority)999 });
        result.ShouldHaveValidationErrorFor(x => x.Priority);
    }

    [Fact]
    public void TooLongTitle_FailsValidation()
    {
        var result = _validator.TestValidate(Valid() with { Title = new string('x', 201) });
        result.ShouldHaveValidationErrorFor(x => x.Title);
    }
}
