namespace Bakery.Application.DTOs;

public sealed record CartDto(
    Guid Id,
    Guid CustomerId,
    IReadOnlyList<CartItemDto> Items,
    decimal Subtotal,
    decimal Total);
