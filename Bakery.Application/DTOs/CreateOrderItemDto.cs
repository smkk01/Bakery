namespace Bakery.Application.DTOs;

public sealed record CreateOrderItemDto(Guid ProductId, int Quantity);
