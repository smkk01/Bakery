using Bakery.Application.DTOs;
using Bakery.Application.Mapping;
using Bakery.Application.Repositories;
using Bakery.Domain.Entities;
using Bakery.Domain.Enums;

namespace Bakery.Application.Orders;

public sealed class OrderService(
    IOrderRepository orderRepository,
    IProductRepository productRepository) : IOrderService
{
    private const int MaximumQuantityPerProduct = 10000;
    private const decimal MaximumOrderTotal = 100000000m;

    public async Task<OrderDto> CreateAsync(
        CreateOrderRequestDto request,
        CancellationToken cancellationToken = default)
    {
        ValidateRequest(request);

        var products = await productRepository.GetAllAsync(cancellationToken);
        var productsById = products.ToDictionary(product => product.Id);
        var aggregatedItems = request.Items
            .GroupBy(item => item.ProductId)
            .Select(group => new { ProductId = group.Key, Quantity = group.Sum(item => (long)item.Quantity) })
            .ToArray();

        var orderItems = new List<OrderItem>(aggregatedItems.Length);
        foreach (var requestedItem in aggregatedItems)
        {
            if (requestedItem.Quantity > MaximumQuantityPerProduct)
            {
                throw new ArgumentException(
                    $"The total quantity for product '{requestedItem.ProductId}' cannot exceed {MaximumQuantityPerProduct}.",
                    nameof(request));
            }

            if (!productsById.TryGetValue(requestedItem.ProductId, out var product))
            {
                throw new ArgumentException(
                    $"Product '{requestedItem.ProductId}' does not exist.",
                    nameof(request));
            }

            if (!product.IsAvailable)
            {
                throw new ArgumentException(
                    $"Product '{product.Name}' is not currently available.",
                    nameof(request));
            }

            orderItems.Add(new OrderItem
            {
                ProductId = product.Id,
                ProductName = product.Name,
                Quantity = (int)requestedItem.Quantity,
                UnitPrice = product.Price
            });
        }

        var totalAmount = orderItems.Sum(item => item.Quantity * item.UnitPrice);
        if (totalAmount > MaximumOrderTotal)
        {
            throw new ArgumentException(
                $"Order total cannot exceed {MaximumOrderTotal}.",
                nameof(request));
        }

        var order = new Order
        {
            OrderNumber = GenerateOrderNumber(),
            CustomerId = request.CustomerId,
            OrderDate = DateTime.UtcNow,
            Status = OrderStatus.Pending,
            TotalAmount = totalAmount,
            Items = orderItems
        };

        await orderRepository.CreateAsync(order, cancellationToken);
        return order.ToDto();
    }

    public async Task<OrderDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) =>
        (await orderRepository.GetByIdAsync(id, cancellationToken))?.ToDto();

    public async Task<OrderDto?> GetByOrderNumberAsync(
        string orderNumber,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(orderNumber);
        var orders = await orderRepository.GetAllAsync(cancellationToken);
        return orders.FirstOrDefault(order =>
            string.Equals(order.OrderNumber, orderNumber.Trim(), StringComparison.OrdinalIgnoreCase))?.ToDto();
    }

    public async Task<IReadOnlyList<OrderDto>> GetAllAsync(
        Guid? customerId = null,
        CancellationToken cancellationToken = default)
    {
        if (customerId == Guid.Empty)
        {
            throw new ArgumentException("Customer identifier must be a non-empty GUID.", nameof(customerId));
        }

        var orders = await orderRepository.GetAllAsync(cancellationToken);
        return orders
            .Where(order => customerId is null || order.CustomerId == customerId)
            .OrderByDescending(order => order.OrderDate)
            .Select(order => order.ToDto())
            .ToArray();
    }

    public async Task<OrderDto?> UpdateStatusAsync(
        Guid orderId,
        OrderStatus status,
        CancellationToken cancellationToken = default)
    {
        if (orderId == Guid.Empty)
        {
            throw new ArgumentException("Order identifier must be a non-empty GUID.", nameof(orderId));
        }

        if (!Enum.IsDefined(status))
        {
            throw new ArgumentOutOfRangeException(nameof(status), status, "Unknown order status.");
        }

        var order = await orderRepository.GetByIdAsync(orderId, cancellationToken);
        if (order is null)
        {
            return null;
        }

        order.Status = status;
        order.UpdatedAtUtc = DateTime.UtcNow;
        if (!await orderRepository.UpdateAsync(order, cancellationToken))
        {
            return null;
        }

        return order.ToDto();
    }

    private static void ValidateRequest(CreateOrderRequestDto request)
    {
        ArgumentNullException.ThrowIfNull(request);
        if (request.CustomerId == Guid.Empty)
        {
            throw new ArgumentException("A customer identifier is required.", nameof(request));
        }

        if (request.Items is null || request.Items.Count == 0)
        {
            throw new ArgumentException("An order must contain at least one item.", nameof(request));
        }

        if (request.Items.Any(item => item is null || item.ProductId == Guid.Empty))
        {
            throw new ArgumentException("Every order item must have a product identifier.", nameof(request));
        }

        if (request.Items.Any(item => item.Quantity is < 1 or > MaximumQuantityPerProduct))
        {
            throw new ArgumentException(
                $"Order item quantities must be between 1 and {MaximumQuantityPerProduct}.",
                nameof(request));
        }
    }

    private static string GenerateOrderNumber() =>
        $"BK-{DateTime.UtcNow:yyyyMMdd}-{Guid.NewGuid():N}".ToUpperInvariant();
}
