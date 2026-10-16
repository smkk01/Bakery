using Bakery.Api.DTOs;
using Bakery.Application.Authentication;
using Bakery.Application.Cart;
using Bakery.Application.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Bakery.Api.Controllers;

[ApiController]
[Route("api/cart")]
[Produces("application/json")]
[Tags("Cart")]
[Authorize(Policy = AuthPolicies.CustomerOnly)]
public sealed class CartController(
    ICartService cartService,
    ILogger<CartController> logger) : ControllerBase
{
    /// <summary>Returns a customer's cart and calculated totals.</summary>
    [HttpGet]
    [ProducesResponseType(typeof(CartDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<CartDto>> Get(
        [FromQuery(Name = "customerId")] Guid customerId,
        CancellationToken cancellationToken)
    {
        if (!CanAccessCustomer(customerId))
        {
            return Forbid();
        }

        var cart = await cartService.GetAsync(customerId, cancellationToken);
        logger.LogInformation("Retrieved cart for customer {CustomerId}", customerId);
        return Ok(cart);
    }

    /// <summary>
    /// Adds an existing, available catalog product to a customer's cart, incrementing its quantity if present.
    /// Use the product ID returned by POST /api/products or GET /api/products.
    /// </summary>
    [HttpPost("add")]
    [ProducesResponseType(typeof(CartDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<CartDto>> Add(
        [FromBody] CartItemRequest request,
        CancellationToken cancellationToken)
    {
        if (!CanAccessCustomer(request.CustomerId))
        {
            return Forbid();
        }

        var cart = await cartService.AddAsync(request.ToInput(), cancellationToken);
        if (cart is null)
        {
            logger.LogInformation("Could not add product {ProductId}: product was not found", request.ProductId);
            var problem = new ProblemDetails
            {
                Status = StatusCodes.Status404NotFound,
                Title = "Product not found",
                Detail = $"No catalog product exists with ID '{request.ProductId}'. Create a product first or use an ID from GET /api/products.",
                Instance = HttpContext.Request.Path
            };
            problem.Extensions["productId"] = request.ProductId;
            problem.Extensions["traceId"] = HttpContext.TraceIdentifier;
            return NotFound(problem);
        }

        logger.LogInformation(
            "Added quantity {Quantity} of product {ProductId} to customer {CustomerId}'s cart",
            request.Quantity,
            request.ProductId,
            request.CustomerId);
        return Ok(cart);
    }

    /// <summary>Sets a product's quantity in a customer's cart.</summary>
    [HttpPut("update")]
    [ProducesResponseType(typeof(CartDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<CartDto>> Update(
        [FromBody] CartItemRequest request,
        CancellationToken cancellationToken)
    {
        if (!CanAccessCustomer(request.CustomerId))
        {
            return Forbid();
        }

        var cart = await cartService.UpdateAsync(request.ToInput(), cancellationToken);
        if (cart is null)
        {
            logger.LogInformation(
                "Could not update cart item {ProductId} for customer {CustomerId}: cart or item not found",
                request.ProductId,
                request.CustomerId);
            return NotFound();
        }

        logger.LogInformation(
            "Updated product {ProductId} quantity to {Quantity} in customer {CustomerId}'s cart",
            request.ProductId,
            request.Quantity,
            request.CustomerId);
        return Ok(cart);
    }

    /// <summary>Removes a product from a customer's cart.</summary>
    [HttpDelete("remove")]
    [ProducesResponseType(typeof(CartDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<CartDto>> Remove(
        [FromBody] RemoveCartItemRequest request,
        CancellationToken cancellationToken)
    {
        if (!CanAccessCustomer(request.CustomerId))
        {
            return Forbid();
        }

        var cart = await cartService.RemoveAsync(request.CustomerId, request.ProductId, cancellationToken);
        if (cart is null)
        {
            logger.LogInformation(
                "Could not remove product {ProductId} for customer {CustomerId}: cart or item not found",
                request.ProductId,
                request.CustomerId);
            return NotFound();
        }

        logger.LogInformation(
            "Removed product {ProductId} from customer {CustomerId}'s cart",
            request.ProductId,
            request.CustomerId);
        return Ok(cart);
    }

    private bool CanAccessCustomer(Guid customerId) =>
        Guid.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var authenticatedCustomerId) &&
        authenticatedCustomerId == customerId;
}
