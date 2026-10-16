using Bakery.Api.DTOs;
using Bakery.Application.Authentication;
using Bakery.Application.DTOs;
using Bakery.Application.Products;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Bakery.Api.Controllers;

[ApiController]
[Route("api/products")]
[Produces("application/json")]
public sealed class ProductsController(
    IProductService productService,
    ILogger<ProductsController> logger) : ControllerBase
{
    /// <summary>Returns all bakery products.</summary>
    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<ProductDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<ProductDto>>> GetAll(CancellationToken cancellationToken)
    {
        var products = await productService.GetAllAsync(cancellationToken);
        logger.LogInformation("Retrieved {ProductCount} products", products.Count);
        return Ok(products);
    }

    /// <summary>Returns a product by its identifier.</summary>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(ProductDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ProductDto>> GetById(Guid id, CancellationToken cancellationToken)
    {
        var product = await productService.GetByIdAsync(id, cancellationToken);
        if (product is null)
        {
            logger.LogInformation("Product {ProductId} was not found", id);
            return NotFound();
        }

        logger.LogInformation("Retrieved product {ProductId}", id);
        return Ok(product);
    }

    /// <summary>Creates a product in the catalog.</summary>
    [HttpPost]
[Authorize(Policy = AuthPolicies.AdminOnly)]
    [ProducesResponseType(typeof(ProductDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<ProductDto>> Create(
        [FromBody] ProductRequest request,
        CancellationToken cancellationToken)
    {
        var product = await productService.CreateAsync(request.ToInput(), cancellationToken);
        logger.LogInformation("Created product {ProductId} in category {CategoryId}", product.Id, product.CategoryId);
        return CreatedAtAction(nameof(GetById), new { id = product.Id }, product);
    }

    /// <summary>Replaces the editable fields of a product.</summary>
    [HttpPut("{id:guid}")]
[Authorize(Policy = AuthPolicies.AdminOnly)]
    [ProducesResponseType(typeof(ProductDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<ProductDto>> Update(
        Guid id,
        [FromBody] ProductRequest request,
        CancellationToken cancellationToken)
    {
        var product = await productService.UpdateAsync(id, request.ToInput(), cancellationToken);
        if (product is null)
        {
            logger.LogInformation("Could not update product {ProductId}: product was not found", id);
            return NotFound();
        }

        logger.LogInformation("Updated product {ProductId}", id);
        return Ok(product);
    }

    /// <summary>Deletes a product from the catalog.</summary>
    [HttpDelete("{id:guid}")]
[Authorize(Policy = AuthPolicies.AdminOnly)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        var deleted = await productService.DeleteAsync(id, cancellationToken);
        if (!deleted)
        {
            logger.LogInformation("Could not delete product {ProductId}: product was not found", id);
            return NotFound();
        }

        logger.LogInformation("Deleted product {ProductId}", id);
        return NoContent();
    }
}
