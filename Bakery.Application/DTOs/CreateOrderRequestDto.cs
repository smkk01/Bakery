namespace Bakery.Application.DTOs;

public sealed record CreateOrderRequestDto(
    Guid CustomerId,
    IReadOnlyList<CreateOrderItemDto> Items);
