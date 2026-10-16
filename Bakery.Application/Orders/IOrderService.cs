using Bakery.Application.DTOs;
using Bakery.Domain.Enums;

namespace Bakery.Application.Orders;

public interface IOrderService
{
    Task<OrderDto> CreateAsync(CreateOrderRequestDto request, CancellationToken cancellationToken = default);

    Task<OrderDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task<OrderDto?> GetByOrderNumberAsync(string orderNumber, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<OrderDto>> GetAllAsync(
        Guid? customerId = null,
        CancellationToken cancellationToken = default);

    Task<OrderDto?> UpdateStatusAsync(
        Guid orderId,
        OrderStatus status,
        CancellationToken cancellationToken = default);
}
