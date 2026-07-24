using AssetFlow.Application.Abstractions;
using AssetFlow.Application.Common;
using AssetFlow.Application.Common.Exceptions;
using AssetFlow.Application.Contracts.WorkOrders;
using AssetFlow.Application.Mapping;
using AssetFlow.Domain.Entities;
using FluentValidation;
using Microsoft.Extensions.Logging;

namespace AssetFlow.Application.Services;

public class WorkOrderService : IWorkOrderService
{
    private readonly IWorkOrderRepository _workOrders;
    private readonly IAssetRepository _assets;
    private readonly IUserRepository _users;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IValidator<CreateWorkOrderRequest> _createValidator;
    private readonly IValidator<UpdateWorkOrderRequest> _updateValidator;
    private readonly ILogger<WorkOrderService> _logger;

    public WorkOrderService(
        IWorkOrderRepository workOrders,
        IAssetRepository assets,
        IUserRepository users,
        IUnitOfWork unitOfWork,
        IValidator<CreateWorkOrderRequest> createValidator,
        IValidator<UpdateWorkOrderRequest> updateValidator,
        ILogger<WorkOrderService> logger)
    {
        _workOrders = workOrders;
        _assets = assets;
        _users = users;
        _unitOfWork = unitOfWork;
        _createValidator = createValidator;
        _updateValidator = updateValidator;
        _logger = logger;
    }

    public async Task<PagedResult<WorkOrderDto>> GetPagedAsync(WorkOrderQuery query, CancellationToken cancellationToken = default)
    {
        var page = await _workOrders.SearchAsync(query, cancellationToken);
        var dtos = page.Items.Select(w => w.ToDto()).ToList();
        return new PagedResult<WorkOrderDto>(dtos, page.TotalCount, page.Page, page.PageSize);
    }

    public async Task<WorkOrderDto> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var workOrder = await _workOrders.GetWithDetailsAsync(id, cancellationToken)
            ?? throw new NotFoundException(nameof(WorkOrder), id);
        return workOrder.ToDto();
    }

    public async Task<WorkOrderDto> CreateAsync(CreateWorkOrderRequest request, CancellationToken cancellationToken = default)
    {
        await _createValidator.ValidateAndThrowAsync(request, cancellationToken);

        if (await _assets.GetByIdAsync(request.AssetId, cancellationToken) is null)
        {
            throw new NotFoundException(nameof(Asset), request.AssetId);
        }

        await EnsureTechnicianExistsAsync(request.AssignedTechnicianId, cancellationToken);

        var workOrder = new WorkOrder
        {
            Title = request.Title.Trim(),
            Description = request.Description?.Trim(),
            Priority = request.Priority,
            Status = WorkOrderStatus.Open,
            AssetId = request.AssetId,
            AssignedTechnicianId = request.AssignedTechnicianId,
            DueDate = request.DueDate
        };

        await _workOrders.AddAsync(workOrder, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Created work order {WorkOrderId} for asset {AssetId}", workOrder.Id, workOrder.AssetId);

        // Reload with navigation properties so the response carries asset and technician names.
        var created = await _workOrders.GetWithDetailsAsync(workOrder.Id, cancellationToken);
        return (created ?? workOrder).ToDto();
    }

    public async Task<WorkOrderDto> UpdateAsync(Guid id, UpdateWorkOrderRequest request, CancellationToken cancellationToken = default)
    {
        await _updateValidator.ValidateAndThrowAsync(request, cancellationToken);

        var workOrder = await _workOrders.GetByIdAsync(id, cancellationToken)
            ?? throw new NotFoundException(nameof(WorkOrder), id);

        await EnsureTechnicianExistsAsync(request.AssignedTechnicianId, cancellationToken);

        workOrder.Title = request.Title.Trim();
        workOrder.Description = request.Description?.Trim();
        workOrder.Priority = request.Priority;
        workOrder.Status = request.Status;
        workOrder.AssignedTechnicianId = request.AssignedTechnicianId;
        workOrder.DueDate = request.DueDate;

        _workOrders.Update(workOrder);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var updated = await _workOrders.GetWithDetailsAsync(id, cancellationToken);
        return (updated ?? workOrder).ToDto();
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var workOrder = await _workOrders.GetByIdAsync(id, cancellationToken)
            ?? throw new NotFoundException(nameof(WorkOrder), id);

        _workOrders.Remove(workOrder);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        _logger.LogInformation("Deleted work order {WorkOrderId}", id);
    }

    private async Task EnsureTechnicianExistsAsync(Guid? technicianId, CancellationToken cancellationToken)
    {
        if (technicianId is Guid id && !await _users.ExistsAsync(id, cancellationToken))
        {
            throw new NotFoundException(nameof(User), id);
        }
    }
}
