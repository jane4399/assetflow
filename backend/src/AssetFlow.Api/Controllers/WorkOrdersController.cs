using AssetFlow.Application.Abstractions;
using AssetFlow.Application.Common;
using AssetFlow.Application.Contracts.WorkOrders;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AssetFlow.Api.Controllers;

/// <summary>
/// CRUD for work orders. Any authenticated user can read; technicians and admins
/// can create/update; only admins can delete.
/// </summary>
[ApiController]
[Route("api/workorders")]
[Authorize]
[Produces("application/json")]
public class WorkOrdersController : ControllerBase
{
    private readonly IWorkOrderService _workOrderService;

    public WorkOrdersController(IWorkOrderService workOrderService)
    {
        _workOrderService = workOrderService;
    }

    /// <summary>Lists work orders with paging, sorting and filtering by status, priority, asset and technician.</summary>
    [HttpGet]
    [ProducesResponseType(typeof(PagedResult<WorkOrderDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<PagedResult<WorkOrderDto>>> GetAll([FromQuery] WorkOrderQuery query, CancellationToken cancellationToken)
    {
        var result = await _workOrderService.GetPagedAsync(query, cancellationToken);
        return Ok(result);
    }

    /// <summary>Gets a single work order by id.</summary>
    [HttpGet("{id:guid}", Name = "GetWorkOrderById")]
    [ProducesResponseType(typeof(WorkOrderDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<WorkOrderDto>> GetById(Guid id, CancellationToken cancellationToken)
    {
        var workOrder = await _workOrderService.GetByIdAsync(id, cancellationToken);
        return Ok(workOrder);
    }

    /// <summary>Creates a work order (Technician or Admin).</summary>
    [HttpPost]
    [Authorize(Policy = "RequireTechnician")]
    [ProducesResponseType(typeof(WorkOrderDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<WorkOrderDto>> Create(CreateWorkOrderRequest request, CancellationToken cancellationToken)
    {
        var created = await _workOrderService.CreateAsync(request, cancellationToken);
        return CreatedAtRoute("GetWorkOrderById", new { id = created.Id }, created);
    }

    /// <summary>Updates a work order (Technician or Admin).</summary>
    [HttpPut("{id:guid}")]
    [Authorize(Policy = "RequireTechnician")]
    [ProducesResponseType(typeof(WorkOrderDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<WorkOrderDto>> Update(Guid id, UpdateWorkOrderRequest request, CancellationToken cancellationToken)
    {
        var updated = await _workOrderService.UpdateAsync(id, request, cancellationToken);
        return Ok(updated);
    }

    /// <summary>Deletes a work order (Admin only).</summary>
    [HttpDelete("{id:guid}")]
    [Authorize(Policy = "RequireAdmin")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        await _workOrderService.DeleteAsync(id, cancellationToken);
        return NoContent();
    }
}
