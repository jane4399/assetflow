using AssetFlow.Application.Abstractions;
using AssetFlow.Application.Common;
using AssetFlow.Application.Contracts.Assets;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AssetFlow.Api.Controllers;

/// <summary>CRUD for assets. Reads are open to any authenticated user; writes require Admin.</summary>
[ApiController]
[Route("api/assets")]
[Authorize]
[Produces("application/json")]
public class AssetsController : ControllerBase
{
    private readonly IAssetService _assetService;

    public AssetsController(IAssetService assetService)
    {
        _assetService = assetService;
    }

    /// <summary>Lists assets with paging, sorting and filtering by status and site.</summary>
    [HttpGet]
    [ProducesResponseType(typeof(PagedResult<AssetDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<PagedResult<AssetDto>>> GetAll([FromQuery] AssetQuery query, CancellationToken cancellationToken)
    {
        var result = await _assetService.GetPagedAsync(query, cancellationToken);
        return Ok(result);
    }

    /// <summary>Gets a single asset by id.</summary>
    [HttpGet("{id:guid}", Name = "GetAssetById")]
    [ProducesResponseType(typeof(AssetDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<AssetDto>> GetById(Guid id, CancellationToken cancellationToken)
    {
        var asset = await _assetService.GetByIdAsync(id, cancellationToken);
        return Ok(asset);
    }

    /// <summary>Creates an asset (Admin only).</summary>
    [HttpPost]
    [Authorize(Policy = "RequireAdmin")]
    [ProducesResponseType(typeof(AssetDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<AssetDto>> Create(CreateAssetRequest request, CancellationToken cancellationToken)
    {
        var created = await _assetService.CreateAsync(request, cancellationToken);
        return CreatedAtRoute("GetAssetById", new { id = created.Id }, created);
    }

    /// <summary>Updates an asset (Admin only).</summary>
    [HttpPut("{id:guid}")]
    [Authorize(Policy = "RequireAdmin")]
    [ProducesResponseType(typeof(AssetDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<AssetDto>> Update(Guid id, UpdateAssetRequest request, CancellationToken cancellationToken)
    {
        var updated = await _assetService.UpdateAsync(id, request, cancellationToken);
        return Ok(updated);
    }

    /// <summary>Deletes an asset (Admin only).</summary>
    [HttpDelete("{id:guid}")]
    [Authorize(Policy = "RequireAdmin")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        await _assetService.DeleteAsync(id, cancellationToken);
        return NoContent();
    }
}
