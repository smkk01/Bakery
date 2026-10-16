using Bakery.Api.DTOs;
using Bakery.Application.Authentication;
using Bakery.Application.DTOs;
using Bakery.Application.Orders;
using Bakery.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Bakery.Api.Controllers;

[ApiController]
[Route("api/orders")]
[Produces("application/json")]
[Tags("Orders")]
[Authorize(Policy = AuthPolicies.CustomerOrAdmin)]
public sealed class OrdersController(
    IOrderService orderService,
    ILogger<OrdersController> logger) : ControllerBase
{
    /// <summary>Creates an order from available catalog products.</summary>
    [HttpPost]
[Authorize(Policy = AuthPolicies.CustomerOnly)]
    [ProducesResponseType(typeof(OrderDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<OrderDto>> Create(
        [FromBody] CreateOrderRequest request,
        CancellationToken cancellationToken)
    {
        if (!IsCurrentCustomer(request.CustomerId))
        {
            return Forbid();
        }

        var order = await orderService.CreateAsync(request.ToApplicationDto(), cancellationToken);
        logger.LogInformation(
            "Created order {OrderNumber} ({OrderId}) for customer {CustomerId}, total {TotalAmount}",
            order.OrderNumber,
            order.Id,
            order.CustomerId,
            order.TotalAmount);

        return CreatedAtAction(nameof(GetById), new { id = order.Id }, order);
    }

    /// <summary>Returns orders, optionally filtered by customer identifier.</summary>
    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<OrderDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<IReadOnlyList<OrderDto>>> GetAll(
        [FromQuery] Guid? customerId,
        CancellationToken cancellationToken)
    {
        var isAdmin = User.IsInRole(UserRole.Admin.ToString());
        if (!isAdmin)
        {
            if (!Guid.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var authenticatedCustomerId))
            {
                return Forbid();
            }

            if (customerId.HasValue && customerId.Value != authenticatedCustomerId)
            {
                return Forbid();
            }

            customerId = authenticatedCustomerId;
        }

        var orders = await orderService.GetAllAsync(customerId, cancellationToken);
        logger.LogInformation("Retrieved {OrderCount} orders", orders.Count);
        return Ok(orders);
    }

    /// <summary>Returns an order by its identifier.</summary>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(OrderDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<OrderDto>> GetById(Guid id, CancellationToken cancellationToken)
    {
        var order = await orderService.GetByIdAsync(id, cancellationToken);
        if (order is null)
        {
            logger.LogInformation("Order {OrderId} was not found", id);
            return NotFound();
        }

        if (!CanAccessOrder(order))
        {
            return Forbid();
        }

        return Ok(order);
    }

    /// <summary>Returns an order by its generated order number.</summary>
    [HttpGet("number/{orderNumber}")]
    [ProducesResponseType(typeof(OrderDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<OrderDto>> GetByOrderNumber(
        string orderNumber,
        CancellationToken cancellationToken)
    {
        var order = await orderService.GetByOrderNumberAsync(orderNumber, cancellationToken);
        if (order is null)
        {
            logger.LogInformation("Order number {OrderNumber} was not found", orderNumber);
            return NotFound();
        }

        if (!CanAccessOrder(order))
        {
            return Forbid();
        }

        return Ok(order);
    }

    /// <summary>Updates an order's status. Available to administrators only.</summary>
    [HttpPut("{id:guid}/status")]
    [Authorize(Policy = AuthPolicies.AdminOnly)]
    [ProducesResponseType(typeof(OrderDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<OrderDto>> UpdateStatus(
        Guid id,
        [FromBody] UpdateOrderStatusRequest request,
        CancellationToken cancellationToken)
    {
        var order = await orderService.UpdateStatusAsync(id, request.Status, cancellationToken);
        if (order is null)
        {
            logger.LogInformation("Could not update order {OrderId}: order was not found", id);
            return NotFound();
        }

        logger.LogInformation("Updated status for order {OrderId} to {OrderStatus}", id, order.Status);
        return Ok(order);
    }

    private bool IsCurrentCustomer(Guid customerId) =>
        Guid.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var authenticatedCustomerId) &&
        authenticatedCustomerId == customerId;

    private bool CanAccessOrder(OrderDto order) =>
        User.IsInRole(UserRole.Admin.ToString()) || IsCurrentCustomer(order.CustomerId);
}
