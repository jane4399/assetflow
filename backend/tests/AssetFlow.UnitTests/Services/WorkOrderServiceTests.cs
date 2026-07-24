using AssetFlow.Application.Abstractions;
using AssetFlow.Application.Common.Exceptions;
using AssetFlow.Application.Contracts.WorkOrders;
using AssetFlow.Application.Services;
using AssetFlow.Application.Validators;
using AssetFlow.Domain.Entities;
using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Xunit;

namespace AssetFlow.UnitTests.Services;

public class WorkOrderServiceTests
{
    private readonly Mock<IWorkOrderRepository> _workOrders = new();
    private readonly Mock<IAssetRepository> _assets = new();
    private readonly Mock<IUserRepository> _users = new();
    private readonly Mock<IUnitOfWork> _unitOfWork = new();

    private WorkOrderService CreateSut() => new(
        _workOrders.Object,
        _assets.Object,
        _users.Object,
        _unitOfWork.Object,
        new CreateWorkOrderRequestValidator(),
        new UpdateWorkOrderRequestValidator(),
        NullLogger<WorkOrderService>.Instance);

    [Fact]
    public async Task CreateAsync_WithValidRequest_PersistsAndReturnsDto()
    {
        var assetId = Guid.NewGuid();
        var asset = new Asset { Id = assetId, Name = "Feed Pump A", Tag = "PMP-1001", SiteId = Guid.NewGuid() };

        _assets
            .Setup(r => r.GetByIdAsync(assetId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(asset);

        WorkOrder? captured = null;
        _workOrders
            .Setup(r => r.AddAsync(It.IsAny<WorkOrder>(), It.IsAny<CancellationToken>()))
            .Callback<WorkOrder, CancellationToken>((w, _) => captured = w)
            .Returns(Task.CompletedTask);

        _workOrders
            .Setup(r => r.GetWithDetailsAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(() =>
            {
                captured!.Asset = asset;
                return captured;
            });

        var sut = CreateSut();
        var request = new CreateWorkOrderRequest(
            "Replace mechanical seal",
            "Seal weeping on the outboard side.",
            WorkOrderPriority.High,
            assetId,
            AssignedTechnicianId: null,
            DueDate: DateTime.UtcNow.AddDays(2));

        var result = await sut.CreateAsync(request);

        result.Title.Should().Be("Replace mechanical seal");
        result.Status.Should().Be(nameof(WorkOrderStatus.Open));
        result.Priority.Should().Be(nameof(WorkOrderPriority.High));
        result.AssetName.Should().Be("Feed Pump A");

        _workOrders.Verify(r => r.AddAsync(It.IsAny<WorkOrder>(), It.IsAny<CancellationToken>()), Times.Once);
        _unitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task CreateAsync_WhenAssetDoesNotExist_ThrowsNotFound()
    {
        _assets
            .Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Asset?)null);

        var sut = CreateSut();
        var request = new CreateWorkOrderRequest(
            "Title", null, WorkOrderPriority.Low, Guid.NewGuid(), null, null);

        var act = () => sut.CreateAsync(request);

        await act.Should().ThrowAsync<NotFoundException>();
        _unitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task CreateAsync_WhenAssignedTechnicianMissing_ThrowsNotFound()
    {
        var assetId = Guid.NewGuid();
        _assets
            .Setup(r => r.GetByIdAsync(assetId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Asset { Id = assetId, Name = "Pump", Tag = "P1", SiteId = Guid.NewGuid() });
        _users
            .Setup(r => r.ExistsAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        var sut = CreateSut();
        var request = new CreateWorkOrderRequest(
            "Title", null, WorkOrderPriority.Low, assetId, Guid.NewGuid(), null);

        var act = () => sut.CreateAsync(request);

        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task CreateAsync_WithInvalidRequest_ThrowsValidationException()
    {
        var sut = CreateSut();
        var request = new CreateWorkOrderRequest(
            Title: "", Description: null, Priority: WorkOrderPriority.Low, AssetId: Guid.Empty, AssignedTechnicianId: null, DueDate: null);

        var act = () => sut.CreateAsync(request);

        await act.Should().ThrowAsync<FluentValidation.ValidationException>();
        _workOrders.Verify(r => r.AddAsync(It.IsAny<WorkOrder>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task GetByIdAsync_WhenMissing_ThrowsNotFound()
    {
        _workOrders
            .Setup(r => r.GetWithDetailsAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((WorkOrder?)null);

        var sut = CreateSut();

        var act = () => sut.GetByIdAsync(Guid.NewGuid());

        await act.Should().ThrowAsync<NotFoundException>();
    }
}
