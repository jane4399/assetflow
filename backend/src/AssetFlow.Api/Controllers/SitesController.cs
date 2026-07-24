using AssetFlow.Application.Abstractions;
using AssetFlow.Application.Common;
using AssetFlow.Application.Contracts.Sites;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AssetFlow.Api.Controllers;

/// <summary>CRUD for sites. Reads are open to any authenticated user; writes require Admin.</summary>
[ApiController]
[Route("api/sites")]
[Authorize]
[Produces("application/json")]
public class SitesController : ControllerBase
{
    private readonly ISiteService _siteService;

    public SitesController(ISiteService siteService)
    {
        _siteService = siteService;
    }

    /// <summary>Lists sites with paging, sorting and free-text search.</summary>
    [HttpGet]
    [ProducesResponseType(typeof(PagedResult<SiteDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<PagedResult<SiteDto>>> GetAll([FromQuery] SiteQuery query, CancellationToken cancellationToken)
    {
        var result = await _siteService.GetPagedAsync(query, cancellationToken);
        return Ok(result);
    }

    /// <summary>Gets a single site by id.</summary>
    [HttpGet("{id:guid}", Name = "GetSiteById")]
    [ProducesResponseType(typeof(SiteDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<SiteDto>> GetById(Guid id, CancellationToken cancellationToken)
    {
        var site = await _siteService.GetByIdAsync(id, cancellationToken);
        return Ok(site);
    }

    /// <summary>Creates a site (Admin only).</summary>
    [HttpPost]
    [Authorize(Policy = "RequireAdmin")]
    [ProducesResponseType(typeof(SiteDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<SiteDto>> Create(CreateSiteRequest request, CancellationToken cancellationToken)
    {
        var created = await _siteService.CreateAsync(request, cancellationToken);
        return CreatedAtRoute("GetSiteById", new { id = created.Id }, created);
    }

    /// <summary>Updates a site (Admin only).</summary>
    [HttpPut("{id:guid}")]
    [Authorize(Policy = "RequireAdmin")]
    [ProducesResponseType(typeof(SiteDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<SiteDto>> Update(Guid id, UpdateSiteRequest request, CancellationToken cancellationToken)
    {
        var updated = await _siteService.UpdateAsync(id, request, cancellationToken);
        return Ok(updated);
    }

    /// <summary>Deletes a site (Admin only).</summary>
    [HttpDelete("{id:guid}")]
    [Authorize(Policy = "RequireAdmin")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        await _siteService.DeleteAsync(id, cancellationToken);
        return NoContent();
    }
}
