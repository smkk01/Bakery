using Bakery.Api.DTOs;
using Bakery.Application.Authentication;
using Bakery.Application.DTOs;
using Bakery.Application.Mapping;
using Bakery.Application.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Bakery.Api.Controllers;

[ApiController]
[Route("api/categories")]
[Produces("application/json")]
[Tags("Categories")]
public sealed class CategoriesController(
    ICategoryRepository categoryRepository,
    ILogger<CategoriesController> logger) : ControllerBase
{
    /// <summary>Returns all product categories.</summary>
    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<CategoryDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<CategoryDto>>> GetAll(CancellationToken cancellationToken)
    {
        var categories = await categoryRepository.GetAllAsync(cancellationToken);
        var response = categories.Select(category => category.ToDto()).ToArray();
        logger.LogInformation("Retrieved {CategoryCount} categories", response.Length);
        return Ok(response);
    }

    /// <summary>Returns a category by its identifier.</summary>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(CategoryDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<CategoryDto>> GetById(Guid id, CancellationToken cancellationToken)
    {
        var category = await categoryRepository.GetByIdAsync(id, cancellationToken);
        if (category is null)
        {
            logger.LogInformation("Category {CategoryId} was not found", id);
            return NotFound();
        }

        return Ok(category.ToDto());
    }

    /// <summary>Creates a product category.</summary>
    [HttpPost]
[Authorize(Policy = AuthPolicies.AdminOnly)]
    [ProducesResponseType(typeof(CategoryDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<CategoryDto>> Create(
        [FromBody] CategoryRequest request,
        CancellationToken cancellationToken)
    {
        var category = request.ToEntity();
        await categoryRepository.CreateAsync(category, cancellationToken);
        logger.LogInformation("Created category {CategoryId}", category.Id);
        return CreatedAtAction(nameof(GetById), new { id = category.Id }, category.ToDto());
    }

    /// <summary>Updates an existing product category.</summary>
    [HttpPut("{id:guid}")]
[Authorize(Policy = AuthPolicies.AdminOnly)]
    [ProducesResponseType(typeof(CategoryDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<CategoryDto>> Update(
        Guid id,
        [FromBody] CategoryRequest request,
        CancellationToken cancellationToken)
    {
        var existing = await categoryRepository.GetByIdAsync(id, cancellationToken);
        if (existing is null)
        {
            logger.LogInformation("Could not update category {CategoryId}: category was not found", id);
            return NotFound();
        }

        var updated = request.ToEntity(existing.Id, existing.CreatedAtUtc);
        var saved = await categoryRepository.UpdateAsync(updated, cancellationToken);
        if (!saved)
        {
            logger.LogInformation("Could not update category {CategoryId}: category no longer exists", id);
            return NotFound();
        }

        logger.LogInformation("Updated category {CategoryId}", id);
        return Ok(updated.ToDto());
    }

    /// <summary>Deletes a product category.</summary>
    [HttpDelete("{id:guid}")]
[Authorize(Policy = AuthPolicies.AdminOnly)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        var deleted = await categoryRepository.DeleteAsync(id, cancellationToken);
        if (!deleted)
        {
            logger.LogInformation("Could not delete category {CategoryId}: category was not found", id);
            return NotFound();
        }

        logger.LogInformation("Deleted category {CategoryId}", id);
        return NoContent();
    }
}
