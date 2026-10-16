using Bakery.Domain.Enums;

namespace Bakery.Application.DTOs;

public sealed record OrderDto(
    Guid Id,
    string OrderNumber,
    Guid CustomerId,
    DateTime OrderDate,
    OrderStatus Status,
    decimal TotalAmount,
    IReadOnlyList<OrderItemDto> Items);
